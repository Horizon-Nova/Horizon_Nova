using HNB.IntelligentSystems.GroundingDINO.Models;
using HNB.IntelligentSystems.GroundingDINO.Module;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;

namespace HNB.IntelligentSystems.GroundingDINO.Api;

/// <summary>
/// GroundingDINO 物件檢測 API Controller
/// 統一返回格式：所有 API 都返回 images 陣列格式
/// base64 輸入 → base64 輸出，圖片輸入 → URL 輸出
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroundingDINOApiController(GroundingDINOModule module, IWebHostEnvironment env) : ControllerBase
{
    private const string StorageRoot = "Areas/Backoffice/storage/GroundingDINO";

    /// <summary>
    /// 簡介：檢查 GroundingDINO 服務狀態
    /// 端口：GET /api/GroundingDINOApi/Status
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
    /// 端口：POST /api/GroundingDINOApi/DetectBase64
    /// </summary>
    [HttpPost("DetectBase64")]
    public IActionResult DetectBase64([FromBody] GroundingDINORequest request)
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
    /// 端口：POST /api/GroundingDINOApi/DetectImage
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
                
                ImageUtils.SaveImageFromBytes(detection.CroppedImageBytes, croppedFilePath);
                
                var croppedImageUrl = $"/storage/GroundingDINO/裁剪/{dateStr}/{croppedFileName}";
                detectionsOutput.Add(new
                {
                    id = detection.Id,
                    box = detection.Box,
                    label = detection.Label,
                    score = detection.Score,
                    croppedImageUrl = croppedImageUrl
                });
            }

            using var annotatedImage = RenderDetections(imageBytes, detections);
            using (annotatedImage)
            {
                var annotatedDateStr = DateTime.Now.ToString("yyyy_MM_dd");
                var savePath = Path.Combine(env.ContentRootPath, StorageRoot, "渲染", annotatedDateStr);
                Directory.CreateDirectory(savePath);

                var fileName = $"{imageId}.jpg";
                var filePath = Path.Combine(savePath, fileName);
                ImageUtils.SaveImage(annotatedImage, filePath);

                var annotatedImageUrl = $"/storage/GroundingDINO/渲染/{annotatedDateStr}/{fileName}";
                imageResults.Add(new { imageId, success = true, detections = detectionsOutput, count = detectionsOutput.Count, annotatedImageUrl });
            }
        }

        return Ok(BuildResponse(imageResults, errors));
    }

    private Image<Rgb24> RenderDetections(byte[] imageBytes, List<DetectionResult> detections)
    {
        using var originalImage = ImageUtils.DecodeImage(imageBytes);
        var annotatedImage = originalImage.Clone();
        ImageUtils.DrawDetections(annotatedImage, detections);
        return annotatedImage;
    }

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

