using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Module;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using System.Linq;

namespace HNB.IntelligentSystems.ObjectDetection.Api;

/// <summary>
/// 物件辨識 API Controller
/// 統一返回格式：所有 API 都返回 images 陣列格式
/// base64 輸入 → base64 輸出，圖片輸入 → URL 輸出
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ObjectDetectionApiController(ObjectDetectionModule module, IWebHostEnvironment env) : ControllerBase
{
    private const string StorageRoot = "Areas/Backoffice/storage/ObjectDetection";

    /// <summary>
    /// 簡介：檢查物件辨識服務狀態
    /// 端口：GET /api/ObjectDetectionApi/Status
    /// </summary>
    [HttpGet("Status")]
    public IActionResult Status()
    {
        var statusInfo = module.LoadDetailedStatus();
        var (lastCheckTime, consecutiveFailures) = module.GetHealthCheckInfo();

        return Ok(new
        {
            isReady = statusInfo.IsReady,
            message = statusInfo.Message,
            isDownloading = statusInfo.IsDownloading,
            downloadProgress = statusInfo.IsDownloading ? new
            {
                currentFiles = statusInfo.CurrentDownloadingFiles,
                overallProgress = statusInfo.OverallProgress,
                fileProgress = statusInfo.FileProgress,
                totalDownloadedBytes = statusInfo.TotalDownloadedBytes,
                totalBytes = statusInfo.TotalBytes,
                queue = statusInfo.DownloadQueue
            } : null,
            healthCheck = new
            {
                lastCheckTime,
                consecutiveFailures,
                autoRepairEnabled = true
            }
        });
    }

    /// <summary>
    /// 簡介：從 base64 圖片進行物件檢測，返回檢測結果和裁剪圖片 base64
    /// 端口：POST /api/ObjectDetectionApi/DetectBase64
    /// </summary>
    [HttpPost("DetectBase64")]
    public IActionResult DetectBase64([FromBody] ObjectDetectionRequest request)
    {
        if (request.ImagesBase64 == null || request.ImagesBase64.Count == 0)
            return BadRequest(new { success = false, error = "請提供 base64 圖片資料" });

        var imageResults = new List<dynamic>();
        var errors = new List<string>();

        var (validImages, parseErrors) = ImageUtils.ParseBase64Images(request.ImagesBase64);
        
        foreach (var parseError in parseErrors)
        {
            errors.Add(parseError);
            imageResults.Add(new { imageId = $"img_{Guid.NewGuid():N}", success = false, error = parseError, detections = new List<object>(), count = 0 });
        }

        foreach (var imageBytes in validImages)
        {
            var detections = module.DetectObjectsFromBytes(imageBytes, request.TextPrompt);
            var imageId = $"img_{Guid.NewGuid():N}";
            var processedDetections = module.ProcessDetections(imageBytes, detections, imageId);
            var detectionsOutput = processedDetections.Select(d => new
            {
                id = d.Id,
                box = d.Box,
                label = d.Label,
                score = d.Score,
                croppedImageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(d.CroppedImageBytes)}"
            }).ToList();

            imageResults.Add(new { imageId, success = true, detections = detectionsOutput, count = detectionsOutput.Count });
        }

        return Ok(BuildResponse(imageResults, errors));
    }

    /// <summary>
    /// 簡介：從上傳的圖片進行物件檢測，返回渲染後的圖片 URL 和裁剪圖片
    /// 端口：POST /api/ObjectDetectionApi/DetectImage
    /// </summary>
    [HttpPost("DetectImage")]
    public async Task<IActionResult> DetectImage([FromForm(Name = "image[]")] List<IFormFile> images, [FromForm] string? textPrompt = null)
    {
        if (images == null || images.Count == 0)
            return BadRequest(new { success = false, error = "請提供圖片文件" });

        var imageResults = new List<dynamic>();
        var errors = new List<string>();

        var (validImages, readErrors) = await ImageUtils.ReadFormFileImages(images);
        
        foreach (var readError in readErrors)
        {
            errors.Add(readError);
            imageResults.Add(new { imageId = $"img_{Guid.NewGuid():N}", success = false, error = readError, detections = new List<object>(), count = 0 });
        }

        foreach (var imageBytes in validImages)
        {
            var detections = module.DetectObjectsFromBytes(imageBytes, textPrompt);
            var imageId = $"img_{Guid.NewGuid():N}";
            var processedDetections = module.ProcessDetections(imageBytes, detections, imageId);
            
            var dateStr = DateTime.Now.ToString("yyyy_MM_dd");
            var croppedPath = Path.Combine(env.ContentRootPath, StorageRoot, "裁剪", dateStr);
            Directory.CreateDirectory(croppedPath);

            var detectionsOutput = new List<object>();
            foreach (var detection in processedDetections)
            {
                var croppedFileName = $"{detection.Id}.jpg";
                var croppedFilePath = Path.Combine(croppedPath, croppedFileName);
                
                if (ImageUtils.SaveImageFromBytes(detection.CroppedImageBytes, croppedFilePath))
                {
                    var croppedImageUrl = $"/storage/ObjectDetection/裁剪/{dateStr}/{croppedFileName}";
                    detectionsOutput.Add(new
                    {
                        id = detection.Id,
                        box = detection.Box,
                        label = detection.Label,
                        score = detection.Score,
                        croppedImageUrl = croppedImageUrl
                    });
                }
            }

            using var annotatedImage = RenderDetections(imageBytes, detections);
            using (annotatedImage)
            {
                var annotatedDateStr = DateTime.Now.ToString("yyyy_MM_dd");
                var savePath = Path.Combine(env.ContentRootPath, StorageRoot, "渲染", annotatedDateStr);
                Directory.CreateDirectory(savePath);

                var fileName = $"{imageId}.jpg";
                var filePath = Path.Combine(savePath, fileName);
                if (!ImageUtils.SaveImage(annotatedImage, filePath))
                    throw new IOException($"圖片保存失敗：{filePath}");

                var annotatedImageUrl = $"/storage/ObjectDetection/渲染/{annotatedDateStr}/{fileName}";
                imageResults.Add(new { imageId, success = true, detections = detectionsOutput, count = detectionsOutput.Count, annotatedImageUrl });
            }
        }

        return Ok(BuildResponse(imageResults, errors));
    }

    /// <summary>
    /// 簡介：渲染檢測結果到圖片
    /// </summary>
    private Mat RenderDetections(byte[] imageBytes, List<DetectionResult> detections)
    {
        using var originalImage = ImageUtils.DecodeImage(imageBytes);
        if (originalImage == null)
            throw new Exception("無法解碼圖片資料");

        var annotatedImage = originalImage.Clone();
        ImageUtils.DrawDetections(annotatedImage, detections);
        return annotatedImage;
    }

    /// <summary>
    /// 簡介：構建統一的 API 回應格式
    /// </summary>
    private object BuildResponse(List<dynamic> imageResults, List<string> errors)
    {
        var images = imageResults.ToList();
        var totalDetections = images.Where(img => img.success == true).Sum(img => (int)img.count);
        var successCount = images.Count(img => img.success == true);
        var failureCount = images.Count(img => img.success == false);

        return new
        {
            success = failureCount == 0,
            images = images,
            summary = new
            {
                totalImages = images.Count,
                totalDetections = totalDetections,
                successCount = successCount,
                failureCount = failureCount
            },
            errors = errors.Count > 0 ? errors : null
        };
    }
}
