using HNB.IntelligentSystems.GroundingDINO.Configuration;
using HNB.IntelligentSystems.GroundingDINO.Core;
using HNB.IntelligentSystems.GroundingDINO.Models;
using HNB.IntelligentSystems.GroundingDINO.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HNB.IntelligentSystems.GroundingDINO.Module;

/// <summary>
/// GroundingDINO 物件檢測模組
/// 負責執行物件檢測，模型管理由 ModelHealthChecker 負責
/// </summary>
public class GroundingDINOModule(IConfiguration configuration, IWebHostEnvironment environment, ModelHealthChecker healthChecker) : IDisposable
{
    private readonly GroundingDINOConfig _config = LoadConfig(configuration, environment);
    private readonly ModelHealthChecker _healthChecker = healthChecker ?? throw new ArgumentNullException(nameof(healthChecker));
    
    private static GroundingDINOConfig LoadConfig(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var config = new GroundingDINOConfig();
        configuration.GetSection("GroundingDINO").Bind(config);
        
        var storageRoot = configuration["Storage:Root"] ?? "Areas/Backoffice/storage";
        if (string.IsNullOrEmpty(config.ModelPath))
            config.ModelPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "DINO", "groundingdino", "groundingdino.onnx");
        else if (!Path.IsPathRooted(config.ModelPath))
            config.ModelPath = Path.Combine(environment.ContentRootPath, config.ModelPath);
        
        if (string.IsNullOrEmpty(config.VocabPath))
            config.VocabPath = Path.Combine(environment.ContentRootPath, storageRoot, "AI", "DINO", "groundingdino", "vocab.txt");
        else if (!Path.IsPathRooted(config.VocabPath))
            config.VocabPath = Path.Combine(environment.ContentRootPath, config.VocabPath);
        
        return config;
    }

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
    /// 檢測圖片中的物件（從 Image）
    /// </summary>
    public List<DetectionResult> DetectObjects(Image<Rgb24> image, string? textPrompt = null)
    {
        var engine = _healthChecker.GetEngine();
        if (engine == null)
        {
            var (_, message) = LoadModelStatus();
            throw new Exception(message);
        }

        try
        {
            string prompt = textPrompt ?? _config.TextPrompt;
            var results = engine.Detect(image, prompt);
            _healthChecker.ResetFailureCount();
            return results;
        }
        catch
        {
            _healthChecker.NotifyDetectionFailure();
            throw;
        }
    }

    /// <summary>
    /// 從位元組陣列檢測物件
    /// </summary>
    public List<DetectionResult> DetectObjectsFromBytes(byte[] imageBytes, string? textPrompt = null)
    {
        using var image = Utils.ImageUtils.DecodeImage(imageBytes);
        return DetectObjects(image, textPrompt);
    }

    /// <summary>
    /// 處理檢測結果並生成統一的輸出格式（返回裁剪圖片資料，由調用方決定如何處理）
    /// </summary>
    public List<DetectionOutput> ProcessDetections(byte[] imageBytes, List<DetectionResult> detections, string imageId)
    {
        if (detections == null || detections.Count == 0)
            return new List<DetectionOutput>();

        using var originalImage = ImageUtils.DecodeImage(imageBytes);
        var processedResults = new List<DetectionOutput>();

        for (int i = 0; i < detections.Count; i++)
        {
            var detection = detections[i];
            var objectId = $"{imageId}_obj_{i + 1:D3}";

            using var croppedImage = ImageUtils.CropBox(originalImage, detection.Box);

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
    public GroundingDINOConfig GetConfig() => _config;

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

    #region 模型控制方法

    /// <summary>
    /// 手動啟動模型引擎（載入到記憶體）
    /// </summary>
    public void StartEngine()
    {
        _healthChecker.StartEngine();
    }

    /// <summary>
    /// 手動停止模型引擎（從記憶體卸載）
    /// </summary>
    public void StopEngine()
    {
        _healthChecker.StopEngine();
    }

    #endregion

    #region 資源釋放

    public void Dispose()
    {
    }

    #endregion
}

