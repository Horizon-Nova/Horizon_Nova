using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Module;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Api;

/// <summary>
/// 物件辨識 API 端口（外部使用）
/// 用於 Controller 或外部 API 調用，提供更友好的錯誤處理
/// </summary>
public class ObjectDetectionApi(ObjectDetectionModule module)
{
    /// <summary>
    /// 檢查服務狀態
    /// </summary>
    public (bool IsAvailable, string Message) CheckServiceStatus()
    {
        var (isReady, message) = module.LoadModelStatus();
        return (isReady, message);
    }

    /// <summary>
    /// 檢測圖片中的物件（API 調用）
    /// </summary>
    public (bool success, List<DetectionResult>? results, string? error) Detect(byte[] imageBytes, string? textPrompt = null)
    {
        var (success, results, error) = module.DetectObjectsFromBytes(imageBytes, textPrompt);
        return (success, success ? results : null, error);
    }

    /// <summary>
    /// 檢測圖片中的物件（API 調用，從檔案路徑）
    /// </summary>
    public (bool success, List<DetectionResult>? results, string? error) DetectFromFile(string imagePath, string? textPrompt = null)
    {
        if (string.IsNullOrEmpty(imagePath))
            return (false, null, "圖片路徑不能為空");

        if (!File.Exists(imagePath))
            return (false, null, $"圖片檔案不存在：{imagePath}");

        var imageBytes = File.ReadAllBytes(imagePath);
        return Detect(imageBytes, textPrompt);
    }

    /// <summary>
    /// 辨識圖片並返回處理後的圖片（參考 OpenAI API 設計）
    /// 支援輸入：圖片文件或 base64
    /// 輸出：根據輸入決定返回圖片 URL 或 base64
    /// </summary>
    /// <param name="imageBytes">圖片位元組</param>
    /// <param name="textPrompt">文字提示詞</param>
    /// <param name="savePath">保存路徑（如果提供，則保存文件並返回 URL；否則返回 base64）</param>
    /// <param name="fileName">檔案名稱（用於保存文件）</param>
    /// <param name="webPathPrefix">Web 路徑前綴（用於生成 URL，如 "/storage/ObjectDetection"）</param>
    public (bool success, List<DetectionResult>? detections, string? imageUrl, string? imageBase64, string? error) DetectImage(
        byte[] imageBytes,
        string? textPrompt = null,
        string? savePath = null,
        string? fileName = null,
        string? webPathPrefix = null)
    {
        // 執行檢測
        var (detectSuccess, detections, detectError) = Detect(imageBytes, textPrompt);
        if (!detectSuccess || detections == null)
            return (false, null, null, null, detectError ?? "檢測失敗");

        // 載入原始圖片
        using var originalImage = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (originalImage.Empty())
            return (false, null, null, null, "無法解碼圖片資料");

        // 繪製檢測結果
        using var annotatedImage = originalImage.Clone();
        ImageUtils.DrawDetections(annotatedImage, detections);

        // 根據是否有保存路徑決定輸出格式
        if (!string.IsNullOrEmpty(savePath) && !string.IsNullOrEmpty(fileName))
        {
            // 保存文件並返回 URL
            Directory.CreateDirectory(savePath);
            var filePath = Path.Combine(savePath, fileName);
            Cv2.ImWrite(filePath, annotatedImage);

            var webPath = !string.IsNullOrEmpty(webPathPrefix)
                ? $"{webPathPrefix.TrimEnd('/')}/{fileName}"
                : $"/storage/ObjectDetection/{fileName}";

            return (true, detections, webPath, null, null);
        }
        else
        {
            // 返回 base64
            var imageEncoded = annotatedImage.ImEncode(".png");
            var base64String = Convert.ToBase64String(imageEncoded);
            return (true, detections, null, base64String, null);
        }
    }

    /// <summary>
    /// 從 base64 辨識圖片並返回處理後的圖片
    /// </summary>
    public (bool success, List<DetectionResult>? detections, string? imageBase64, string? error) DetectImageFromBase64(
        string imageBase64,
        string? textPrompt = null)
    {
        if (string.IsNullOrEmpty(imageBase64))
            return (false, null, null, "base64 圖片資料不能為空");

        byte[] imageBytes;
        try
        {
            // 移除可能的 data:image/...;base64, 前綴
            var base64Data = imageBase64.Contains(",") 
                ? imageBase64.Split(',')[1] 
                : imageBase64;
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch
        {
            return (false, null, null, "無效的 base64 圖片資料");
        }

        var (success, detections, _, imageBase64Result, error) = DetectImage(imageBytes, textPrompt);
        return (success, detections, imageBase64Result, error);
    }
}

