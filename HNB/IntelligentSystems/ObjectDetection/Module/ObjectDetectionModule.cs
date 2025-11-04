using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Core;
using HNB.IntelligentSystems.ObjectDetection.Models;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Module;

/// <summary>
/// 物件辨識模組端口（內部使用）
/// 用於專案內部其他模組調用
/// </summary>
public class ObjectDetectionModule(ObjectDetectionConfig config) : IDisposable
{
    private DetectionEngine? _engine;
    private readonly object _lock = new object();
    private string? _initializationError;

    #region 狀態檢查方法

    /// <summary>
    /// 載入模型狀態資訊
    /// </summary>
    public (bool IsReady, string Message) LoadModelStatus()
    {
        if (_engine != null)
            return (true, "模型已準備就緒");

        if (!string.IsNullOrEmpty(_initializationError))
            return (false, _initializationError);

        var modelExists = File.Exists(config.ModelPath);
        var vocabExists = File.Exists(config.VocabPath);

        if (!modelExists && !vocabExists)
            return (false, "模型檔案和詞彙表檔案不存在，請先下載 AI 模型");

        if (!modelExists)
            return (false, "模型檔案不存在，請先下載 AI 模型");

        if (!vocabExists)
            return (false, "詞彙表檔案不存在，請先下載 AI 模型");

        return (false, "模型檔案存在但尚未初始化");
    }

    /// <summary>
    /// 檢查模型是否可用
    /// </summary>
    public bool IsModelReady() => _engine != null;

    #endregion

    #region 核心檢測方法

    /// <summary>
    /// 確保引擎已初始化
    /// </summary>
    private void EnsureEngineInitialized()
    {
        if (_engine != null)
            return;

        lock (_lock)
        {
            if (_engine != null)
                return;

            if (!File.Exists(config.ModelPath))
            {
                _initializationError = $"模型檔案不存在：{config.ModelPath}";
                return;
            }

            if (!File.Exists(config.VocabPath))
            {
                _initializationError = $"詞彙表檔案不存在：{config.VocabPath}";
                return;
            }

            _initializationError = null;
            _engine = new DetectionEngine(config);
        }
    }

    /// <summary>
    /// 檢測圖片中的物件
    /// </summary>
    public (bool success, List<DetectionResult> results, string? error) DetectObjects(Mat image, string? textPrompt = null)
    {
        EnsureEngineInitialized();

        if (_engine == null)
        {
            var (_, message) = LoadModelStatus();
            return (false, new List<DetectionResult>(), message);
        }

        if (image == null || image.Empty())
            return (false, new List<DetectionResult>(), "圖片資料無效");

        string prompt = textPrompt ?? config.TextPrompt;
        var results = _engine.Detect(image, prompt);
        return (true, results, null);
    }

    /// <summary>
    /// 從位元組陣列檢測物件
    /// </summary>
    public (bool success, List<DetectionResult> results, string? error) DetectObjectsFromBytes(byte[] imageBytes, string? textPrompt = null)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return (false, new List<DetectionResult>(), "圖片資料為空");

        using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (mat.Empty())
            return (false, new List<DetectionResult>(), "無法解碼圖片資料");

        return DetectObjects(mat, textPrompt);
    }

    #endregion

    #region 配置方法

    /// <summary>
    /// 載入配置資訊
    /// </summary>
    public ObjectDetectionConfig LoadConfig() => config;

    #endregion

    public void Dispose()
    {
        lock (_lock)
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}

