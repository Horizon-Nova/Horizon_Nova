using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Module;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Api;

/// <summary>
/// 物件辨識 API Controller
/// 提供圖片/base64 的智能處理：傳圖片返回 URL，傳 base64 返回 base64
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ObjectDetectionApiController(ObjectDetectionModule module, IWebHostEnvironment env) : ControllerBase
{
    private const string StorageRoot = "Areas/Backoffice/storage/ObjectDetection";

    /// <summary>
    /// 檢查服務狀態
    /// GET: /api/ObjectDetectionApi/Status
    /// </summary>
    [HttpGet("Status")]
    public IActionResult Status()
    {
        var (isReady, message) = module.LoadModelStatus();
        var isDownloading = module.IsDownloading();
        return Ok(new { isReady, message, isDownloading });
    }

    /// <summary>
    /// 下載 AI 模型文件
    /// POST: /api/ObjectDetectionApi/DownloadModels
    /// </summary>
    [HttpPost("DownloadModels")]
    public async Task<IActionResult> DownloadModels()
    {
        // 檢查是否已經有模型
        var (isReady, _) = module.LoadModelStatus();
        if (isReady)
            return Ok(new { success = true, message = "模型已存在，無需下載" });

        // 檢查是否正在下載
        if (module.IsDownloading())
            return Ok(new { success = false, message = "已有下載任務正在進行中" });

        // 開始下載（不等待完成）
        _ = Task.Run(async () =>
        {
            await module.DownloadModelsAsync();
        });

        return Ok(new { success = true, message = "模型下載已開始，請稍後使用 Status 端點查看進度" });
    }

    /// <summary>
    /// 同步下載模型（會等待下載完成）
    /// POST: /api/ObjectDetectionApi/DownloadModelsSync
    /// </summary>
    [HttpPost("DownloadModelsSync")]
    public async Task<IActionResult> DownloadModelsSync()
    {
        // 檢查是否已經有模型
        var (isReady, _) = module.LoadModelStatus();
        if (isReady)
            return Ok(new { success = true, message = "模型已存在，無需下載" });

        // 檢查是否正在下載
        if (module.IsDownloading())
            return Ok(new { success = false, message = "已有下載任務正在進行中" });

        // 同步下載
        var (success, message) = await module.DownloadModelsAsync();
        return Ok(new { success, message });
    }

    /// <summary>
    /// 基礎檢測（從上傳的圖片）- 只返回檢測結果
    /// POST: /api/ObjectDetectionApi/Detect
    /// </summary>
    [HttpPost("Detect")]
    public async Task<IActionResult> Detect([FromForm] IFormFile image, [FromForm] string? textPrompt = null)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { success = false, error = "請提供圖片文件" });

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        var (success, results, error) = module.DetectObjectsFromBytes(imageBytes, textPrompt);
        
        if (!success)
            return Ok(new { success = false, error });

        return Ok(new { success = true, results });
    }

    /// <summary>
    /// 基礎檢測（從 base64）- 只返回檢測結果
    /// POST: /api/ObjectDetectionApi/DetectBase64
    /// </summary>
    [HttpPost("DetectBase64")]
    public IActionResult DetectBase64([FromBody] DetectBase64Request request)
    {
        if (string.IsNullOrEmpty(request.ImageBase64))
            return BadRequest(new { success = false, error = "base64 圖片資料不能為空" });

        byte[] imageBytes;
        try
        {
            var base64Data = request.ImageBase64.Contains(",") 
                ? request.ImageBase64.Split(',')[1] 
                : request.ImageBase64;
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch
        {
            return BadRequest(new { success = false, error = "無效的 base64 圖片資料" });
        }

        var (success, results, error) = module.DetectObjectsFromBytes(imageBytes, request.TextPrompt);
        
        if (!success)
            return Ok(new { success = false, error });

        return Ok(new { success = true, results });
    }

    /// <summary>
    /// 檢測並返回渲染後的圖片（從上傳的圖片）- 返回圖片 URL
    /// POST: /api/ObjectDetectionApi/DetectImage
    /// </summary>
    [HttpPost("DetectImage")]
    public async Task<IActionResult> DetectImage([FromForm] IFormFile image, [FromForm] string? textPrompt = null)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { success = false, error = "請提供圖片文件" });

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        // 執行檢測
        var (detectSuccess, detections, detectError) = module.DetectObjectsFromBytes(imageBytes, textPrompt);
        if (!detectSuccess)
            return Ok(new { success = false, error = detectError });

        // 載入並渲染圖片
        using var originalImage = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (originalImage.Empty())
            return Ok(new { success = false, error = "無法解碼圖片資料" });

        using var annotatedImage = originalImage.Clone();
        ImageUtils.DrawDetections(annotatedImage, detections);

        // 保存文件並返回 URL
        var dateStr = DateTime.Now.ToString("yyyy_MM_dd");
        var savePath = Path.Combine(env.ContentRootPath, StorageRoot, "渲染", dateStr);
        Directory.CreateDirectory(savePath);
        
        var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(image.FileName)}";
        var filePath = Path.Combine(savePath, fileName);
        Cv2.ImWrite(filePath, annotatedImage);

        var imageUrl = $"/storage/ObjectDetection/渲染/{dateStr}/{fileName}";
        return Ok(new { success = true, detections, imageUrl });
    }

    /// <summary>
    /// 檢測並返回渲染後的圖片（從 base64）- 返回 base64
    /// POST: /api/ObjectDetectionApi/DetectImageBase64
    /// </summary>
    [HttpPost("DetectImageBase64")]
    public IActionResult DetectImageBase64([FromBody] DetectBase64Request request)
    {
        if (string.IsNullOrEmpty(request.ImageBase64))
            return BadRequest(new { success = false, error = "base64 圖片資料不能為空" });

        byte[] imageBytes;
        try
        {
            var base64Data = request.ImageBase64.Contains(",") 
                ? request.ImageBase64.Split(',')[1] 
                : request.ImageBase64;
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch
        {
            return BadRequest(new { success = false, error = "無效的 base64 圖片資料" });
        }

        // 執行檢測
        var (detectSuccess, detections, detectError) = module.DetectObjectsFromBytes(imageBytes, request.TextPrompt);
        if (!detectSuccess)
            return Ok(new { success = false, error = detectError });

        // 載入並渲染圖片
        using var originalImage = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (originalImage.Empty())
            return Ok(new { success = false, error = "無法解碼圖片資料" });

        using var annotatedImage = originalImage.Clone();
        ImageUtils.DrawDetections(annotatedImage, detections);

        // 返回 base64
        var imageEncoded = annotatedImage.ImEncode(".png");
        var imageBase64 = Convert.ToBase64String(imageEncoded);

        return Ok(new { success = true, detections, imageBase64 });
    }
}

/// <summary>
/// Base64 檢測請求模型
/// </summary>
public class DetectBase64Request
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string? TextPrompt { get; set; }
}

