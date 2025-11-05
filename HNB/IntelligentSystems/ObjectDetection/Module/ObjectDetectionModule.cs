using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Core;
using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Module;

/// <summary>
/// 物件辨識模組 - 物件檢測功能模組
/// 負責執行物件檢測，模型管理由 ModelHealthChecker 負責
/// </summary>
public class ObjectDetectionModule(ObjectDetectionConfig config, ModelHealthChecker healthChecker) : IDisposable
{
    // 參數驗證
    private readonly ObjectDetectionConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ModelHealthChecker _healthChecker = healthChecker ?? throw new ArgumentNullException(nameof(healthChecker));

    #region 狀態檢查方法

    /// <summary>
    /// 載入模型狀態資訊（簡化版）
    /// </summary>
    public (bool IsReady, string Message) LoadModelStatus()
    {
        return _healthChecker.GetStatus();
    }

    /// <summary>
    /// 載入 AI 模型詳細狀態資訊
    /// </summary>
    public ModelHealthChecker.ModelStatusInfo LoadDetailedStatus()
    {
        return _healthChecker.GetDetailedStatus();
    }

    /// <summary>
    /// 檢查模型是否可用
    /// </summary>
    public bool IsModelReady() => _healthChecker.GetEngine() != null;

    /// <summary>
    /// 檢查是否正在下載模型
    /// </summary>
    public bool IsDownloading() => _healthChecker.IsDownloading();

    /// <summary>
    /// 取得下載進度資訊（支持並行下載）
    /// </summary>
    public (List<string> currentFiles, double overallProgress, Dictionary<string, (double progress, long downloaded, long? total)> fileProgress, long totalDownloadedBytes, long? totalBytes, List<string> queue) LoadDownloadProgress()
    {
        var status = _healthChecker.GetDetailedStatus();
        return (
            status.CurrentDownloadingFiles,
            status.OverallProgress,
            status.FileProgress,
            status.TotalDownloadedBytes,
            status.TotalBytes,
            status.DownloadQueue
        );
    }

    #endregion

    #region 核心檢測方法

    /// <summary>
    /// 檢測圖片中的物件（從 Mat）
    /// </summary>
    public (bool success, List<DetectionResult> results, string? error) DetectObjects(Mat image, string? textPrompt = null)
    {
        // 從健康檢查服務獲取引擎
        var engine = _healthChecker.GetEngine();

        if (engine == null)
        {
            var (_, message) = LoadModelStatus();
            return (false, new List<DetectionResult>(), message);
        }

        if (image == null || image.Empty())
            return (false, new List<DetectionResult>(), "圖片資料無效");

        try
        {
            string prompt = textPrompt ?? _config.TextPrompt;
            var results = engine.Detect(image, prompt);
            _healthChecker.ResetFailureCount(); // 檢測成功，重置失敗計數
            return (true, results, null);
        }
        catch (Exception ex)
        {
            // 檢測失敗，通知健康檢查服務嘗試修復
            _healthChecker.NotifyDetectionFailure();

            return (false, new List<DetectionResult>(), $"檢測失敗：{ex.Message}。系統已啟動自動修復，請稍後再試。");
        }
    }

    /// <summary>
    /// 從位元組陣列檢測物件
    /// </summary>
    public (bool success, List<DetectionResult> results, string? error) DetectObjectsFromBytes(byte[] imageBytes, string? textPrompt = null)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return (false, new List<DetectionResult>(), "圖片資料為空");

        using var mat = Utils.ImageUtils.DecodeImage(imageBytes);
        if (mat == null)
            return (false, new List<DetectionResult>(), "無法解碼圖片資料");

        return DetectObjects(mat, textPrompt);
    }

    /// <summary>
    /// 處理檢測結果並生成統一的輸出格式（返回裁剪圖片資料，由調用方決定如何處理）
    /// </summary>
    public List<DetectionOutput> ProcessDetections(byte[] imageBytes, List<DetectionResult> detections, string imageId)
    {
        if (imageBytes == null || detections == null || detections.Count == 0)
            return new List<DetectionOutput>();

        using var originalImage = ImageUtils.DecodeImage(imageBytes);
        if (originalImage == null)
            return new List<DetectionOutput>();

        var processedResults = new List<DetectionOutput>();

        for (int i = 0; i < detections.Count; i++)
        {
            var detection = detections[i];
            var objectId = $"{imageId}_obj_{i + 1:D3}";

            // 裁剪圖片
            using var croppedImage = ImageUtils.CropBox(originalImage, detection.Box);

            // 編碼為 JPEG 位元組
            var croppedEncoded = ImageUtils.EncodeImage(croppedImage, ".jpg");

            processedResults.Add(new DetectionOutput
            {
                Id = objectId,
                Box = new[] { detection.Box.X, detection.Box.Y, detection.Box.Width, detection.Box.Height },
                Label = detection.Label,
                Score = detection.Score,
                CroppedImageBytes = croppedEncoded
            });
        }

        return processedResults;
    }

    #endregion

    #region 配置方法

    /// <summary>
    /// 載入配置資訊
    /// </summary>
    public ObjectDetectionConfig LoadConfig() => _config;

    /// <summary>
    /// 載入當前配置的文字提示詞
    /// </summary>
    public string LoadTextPrompt() => _config.TextPrompt;

    /// <summary>
    /// 載入當前配置的閾值設定
    /// </summary>
    public (float boxThreshold, float textThreshold) LoadThresholds()
        => (_config.BoxThreshold, _config.TextThreshold);

    #endregion

    #region 健康檢查方法

    /// <summary>
    /// 取得健康檢查資訊（僅查詢，不觸發）
    /// </summary>
    public (DateTime lastCheckTime, int consecutiveFailures) GetHealthCheckInfo()
    {
        return _healthChecker.GetHealthCheckInfo();
    }

    #endregion

    #region 資源釋放

    public void Dispose()
    {
        // ModelHealthChecker 由 DI 容器管理，不需要在這裡釋放
    }

    #endregion
}

