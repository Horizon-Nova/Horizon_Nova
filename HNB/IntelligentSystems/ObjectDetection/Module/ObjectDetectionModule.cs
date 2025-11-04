using HNB.IntelligentSystems.ObjectDetection.Configuration;
using HNB.IntelligentSystems.ObjectDetection.Core;
using HNB.IntelligentSystems.ObjectDetection.Models;
using HNB.IntelligentSystems.ObjectDetection.Utils;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Module;

/// <summary>
/// 物件辨識模組 - 完整的物件檢測功能模組
/// 提供從基礎檢測到高階圖像處理的所有功能
/// </summary>
public class ObjectDetectionModule : IDisposable
{
    private DetectionEngine? _engine;
    private readonly object _lock = new object();
    private string? _initializationError;
    private bool _isDownloading = false;
    private readonly ObjectDetectionConfig _config;
    
    // 遠程模型 URL
    private const string RemoteModelUrl = "https://horizon-nova.up.railway.app/storage/AI/groundingdino.onnx";
    private const string RemoteVocabUrl = "https://horizon-nova.up.railway.app/storage/AI/vocab.txt";

    /// <summary>
    /// 構造函數 - 創建實例時自動檢查模型並啟動下載
    /// </summary>
    public ObjectDetectionModule(ObjectDetectionConfig config)
    {
        _config = config;
        
        // 應用啟動時立即同步檢查模型並啟動下載
        EnsureEngineInitialized();
    }

    #region 狀態檢查方法

    /// <summary>
    /// 檢查服務狀態（對外統一接口）
    /// </summary>
    public (bool IsAvailable, string Message) CheckServiceStatus()
    {
        return LoadModelStatus();
    }

    /// <summary>
    /// 載入模型狀態資訊
    /// </summary>
    public (bool IsReady, string Message) LoadModelStatus()
    {
        if (_engine != null)
            return (true, "模型已準備就緒");

        if (!string.IsNullOrEmpty(_initializationError))
            return (false, _initializationError);

        var modelExists = File.Exists(_config.ModelPath);
        var vocabExists = File.Exists(_config.VocabPath);

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
    
    /// <summary>
    /// 檢查是否正在下載模型
    /// </summary>
    public bool IsDownloading() => _isDownloading;

    #endregion

    #region 初始化方法

    /// <summary>
    /// 確保引擎已初始化（如果模型不存在則自動下載）
    /// </summary>
    private void EnsureEngineInitialized()
    {
        if (_engine != null)
            return;

        lock (_lock)
        {
            if (_engine != null)
                return;

            // 檢查模型文件是否存在，不存在則啟動自動下載
            if (!File.Exists(_config.ModelPath) || !File.Exists(_config.VocabPath))
            {
                // 如果還沒有在下載中，啟動背景下載
                if (!_isDownloading)
                {
                    _initializationError = "模型檔案不存在，正在從遠程服務器自動下載...";
                    
                    // 在背景執行下載（讓 DownloadModelsAsync 自己管理 _isDownloading）
                    Task.Run(async () =>
                    {
                        try
                        {
                            var (success, message) = await DownloadModelsAsync();
                            if (success)
                            {
                                _initializationError = null;
                                // 下載完成後，嘗試初始化引擎
                                lock (_lock)
                                {
                                    if (_engine == null && File.Exists(_config.ModelPath) && File.Exists(_config.VocabPath))
                                    {
                                        _engine = new DetectionEngine(_config);
                                    }
                                }
                            }
                            else
                            {
                                _initializationError = $"模型下載失敗：{message}";
                            }
                        }
                        catch (Exception ex)
                        {
                            _initializationError = $"模型下載異常：{ex.Message}";
                        }
                    });
                }
                else
                {
                    _initializationError = "模型正在下載中，請稍後再試...";
                }
                return;
            }

            _initializationError = null;
            _engine = new DetectionEngine(_config);
        }
    }

    #endregion

    #region 模型下載方法

    /// <summary>
    /// 下載遠程模型文件（異步）
    /// </summary>
    public async Task<(bool success, string message)> DownloadModelsAsync(IProgress<(string fileName, long downloaded, long? total, double percentage)>? progress = null)
    {
        if (_isDownloading)
            return (false, "已有下載任務正在進行中");

        _isDownloading = true;
        
        try
        {
            // 確保目錄存在
            var modelDir = Path.GetDirectoryName(_config.ModelPath);
            if (!string.IsNullOrEmpty(modelDir) && !Directory.Exists(modelDir))
                Directory.CreateDirectory(modelDir);

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

            // 下載模型文件
            if (!File.Exists(_config.ModelPath))
            {
                var (modelSuccess, modelMessage) = await DownloadFileAsync(
                    httpClient, 
                    RemoteModelUrl, 
                    _config.ModelPath, 
                    "groundingdino.onnx",
                    progress
                );
                
                if (!modelSuccess)
                {
                    _isDownloading = false;
                    return (false, modelMessage);
                }
            }

            // 下載詞彙表文件
            if (!File.Exists(_config.VocabPath))
            {
                var (vocabSuccess, vocabMessage) = await DownloadFileAsync(
                    httpClient, 
                    RemoteVocabUrl, 
                    _config.VocabPath, 
                    "vocab.txt",
                    progress
                );
                
                if (!vocabSuccess)
                {
                    _isDownloading = false;
                    return (false, vocabMessage);
                }
            }

            _isDownloading = false;
            return (true, "模型文件下載完成");
        }
        catch (Exception ex)
        {
            _isDownloading = false;
            return (false, $"下載失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 下載單個文件
    /// </summary>
    private async Task<(bool success, string message)> DownloadFileAsync(
        HttpClient httpClient, 
        string url, 
        string savePath,
        string fileName,
        IProgress<(string fileName, long downloaded, long? total, double percentage)>? progress)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;

                if (progress != null && totalBytes.HasValue)
                {
                    var percentage = (double)downloadedBytes / totalBytes.Value * 100;
                    progress.Report((fileName, downloadedBytes, totalBytes, percentage));
                }
            }

            return (true, $"{fileName} 下載完成");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"{fileName} 下載失敗：{ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"{fileName} 保存失敗：{ex.Message}");
        }
    }

    #endregion

    #region 核心檢測方法

    /// <summary>
    /// 檢測圖片中的物件（從 Mat）
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

        string prompt = textPrompt ?? _config.TextPrompt;
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

    /// <summary>
    /// 檢測圖片中的物件（從檔案路徑）
    /// </summary>
    public (bool success, List<DetectionResult>? results, string? error) DetectFromFile(string imagePath, string? textPrompt = null)
    {
        if (string.IsNullOrEmpty(imagePath))
            return (false, null, "圖片路徑不能為空");

        if (!File.Exists(imagePath))
            return (false, null, $"圖片檔案不存在：{imagePath}");

        var imageBytes = File.ReadAllBytes(imagePath);
        var (success, results, error) = DetectObjectsFromBytes(imageBytes, textPrompt);
        return (success, success ? results : null, error);
    }

    #endregion

    #region 配置方法

    /// <summary>
    /// 載入配置資訊
    /// </summary>
    public ObjectDetectionConfig LoadConfig() => _config;

    /// <summary>
    /// 取得當前配置的文字提示詞
    /// </summary>
    public string GetTextPrompt() => _config.TextPrompt;

    /// <summary>
    /// 取得當前配置的閾值設定
    /// </summary>
    public (float boxThreshold, float textThreshold) GetThresholds() 
        => (_config.BoxThreshold, _config.TextThreshold);

    #endregion

    #region 資源釋放

    public void Dispose()
    {
        lock (_lock)
        {
            _engine?.Dispose();
            _engine = null;
        }
    }

    #endregion
}

