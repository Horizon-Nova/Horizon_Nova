using HNB.IntelligentSystems.GroundingDINO.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HNB.IntelligentSystems.GroundingDINO.Core;

/// <summary>
/// 模型健康檢查服務
/// 負責定期檢查模型文件健康狀態、下載、初始化並自動修復
/// 獨立管理模型完整生命週期
/// </summary>
public class ModelHealthChecker : IDisposable
{
    private DetectionEngine? _engine;
    private readonly object _lock = new object();
    private Timer? _healthCheckTimer;
    private DateTime _lastHealthCheckTime = DateTime.MinValue;
    private DateTime? _engineInitializedTime = null;
    private int _consecutiveFailures = 0;
    private readonly GroundingDINOConfig _config;
    private readonly ILogger<ModelHealthChecker> _logger;
    private string? _initializationError;
    private bool _isDownloading = false;

    private Dictionary<string, (long downloaded, long? total)> _fileProgress = new Dictionary<string, (long, long?)>();
    private List<string> _downloadingFiles = new List<string>();
    private List<string> _downloadQueue = new List<string>();

    private const int MaxConsecutiveFailures = 3;
    private const int HealthCheckIntervalMinutes = 5;

    private const string RemoteModelUrl = "https://horizon-nova.up.railway.app/storage/AI/groundingdino.onnx";
    private const string RemoteVocabUrl = "https://horizon-nova.up.railway.app/storage/AI/vocab.txt";

    public ModelHealthChecker(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ModelHealthChecker> logger)
    {
        _config = LoadConfig(configuration, environment);
        _logger = logger;
        _lastHealthCheckTime = DateTime.Now;

        _logger.LogInformation(
            "[GroundingDINO] ModelHealthChecker 初始化完成。ModelPath={ModelPath} | VocabPath={VocabPath}",
            _config.ModelPath,
            _config.VocabPath);
    }

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

    public DetectionEngine? GetEngine()
    {
        lock (_lock)
        {
            return _engine;
        }
    }

    public class ModelStatusInfo
    {
        public bool IsReady { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDownloading { get; set; }
        public List<string> CurrentDownloadingFiles { get; set; } = new List<string>();
        public Dictionary<string, (double progress, long downloaded, long? total)> FileProgress { get; set; } = new Dictionary<string, (double, long, long?)>();
        public double OverallProgress { get; set; }
        public long TotalDownloadedBytes { get; set; }
        public long? TotalBytes { get; set; }
        public List<string> DownloadQueue { get; set; } = new List<string>();
        public bool ModelFileExists { get; set; }
        public bool VocabFileExists { get; set; }
        public long? ModelFileSize { get; set; }
        public long? VocabFileSize { get; set; }
        public DateTime? LastHealthCheckTime { get; set; }
        public int ConsecutiveFailures { get; set; }
        public string? ModelPath { get; set; }
        public string? VocabPath { get; set; }
        public string? InitializationError { get; set; }
        public DateTime? EngineInitializedTime { get; set; }
    }

    public (bool IsReady, string Message) GetStatus()
    {
        var info = GetDetailedStatus();
        return (info.IsReady, info.Message);
    }

    public ModelStatusInfo GetDetailedStatus()
    {
        lock (_lock)
        {
            long totalDownloaded = 0;
            long? totalSize = null;
            var fileProgress = new Dictionary<string, (double progress, long downloaded, long? total)>();

            foreach (var kvp in _fileProgress)
            {
                var (downloaded, total) = kvp.Value;
                totalDownloaded += downloaded;
                if (total.HasValue)
                {
                    totalSize = (totalSize ?? 0) + total.Value;
                    var progress = total.Value > 0 ? (double)downloaded / total.Value * 100.0 : 0.0;
                    fileProgress[kvp.Key] = (progress, downloaded, total);
                }
                else
                {
                    fileProgress[kvp.Key] = (0.0, downloaded, null);
                }
            }

            var overallProgress = totalSize.HasValue && totalSize.Value > 0
                ? (double)totalDownloaded / totalSize.Value * 100.0
                : 0.0;

            // 在鎖內一次取得，避免多次 IO
            bool modelFileExists = File.Exists(_config.ModelPath);
            bool vocabFileExists = File.Exists(_config.VocabPath);
            long? modelFileSize = modelFileExists ? new FileInfo(_config.ModelPath).Length : null;
            long? vocabFileSize = vocabFileExists ? new FileInfo(_config.VocabPath).Length : null;

            var info = new ModelStatusInfo
            {
                IsReady = _engine != null,
                IsDownloading = _isDownloading,
                CurrentDownloadingFiles = new List<string>(_downloadingFiles),
                FileProgress = fileProgress,
                OverallProgress = overallProgress,
                TotalDownloadedBytes = totalDownloaded,
                TotalBytes = totalSize,
                DownloadQueue = new List<string>(_downloadQueue),
                InitializationError = _initializationError,
                LastHealthCheckTime = _lastHealthCheckTime == DateTime.MinValue ? null : _lastHealthCheckTime,
                ConsecutiveFailures = _consecutiveFailures,
                ModelPath = _config.ModelPath,
                VocabPath = _config.VocabPath,
                EngineInitializedTime = _engineInitializedTime,
                ModelFileExists = modelFileExists,
                VocabFileExists = vocabFileExists,
                ModelFileSize = modelFileSize,
                VocabFileSize = vocabFileSize
            };

            if (_engine != null)
            {
                info.Message = "模型已準備就緒";
                info.EngineInitializedTime = _engineInitializedTime;
            }
            else if (!string.IsNullOrEmpty(_initializationError))
            {
                info.Message = _initializationError;
            }
            else if (!modelFileExists)
            {
                info.Message = $"模型檔案不存在：{_config.ModelPath}";
            }
            else if (!vocabFileExists)
            {
                info.Message = $"詞彙表檔案不存在：{_config.VocabPath}";
            }
            else
            {
                info.Message = "模型尚未初始化";
            }

            return info;
        }
    }

    public bool IsDownloading()
    {
        lock (_lock)
        {
            return _isDownloading;
        }
    }

    private void StartHealthCheckTimer()
    {
        _healthCheckTimer = new Timer(PerformHealthCheck, null,
            TimeSpan.FromMinutes(HealthCheckIntervalMinutes),
            TimeSpan.FromMinutes(HealthCheckIntervalMinutes));
    }

    private void PerformHealthCheck(object? state)
    {
        _lastHealthCheckTime = DateTime.Now;

        lock (_lock)
        {
            if (_engine != null)
            {
                _consecutiveFailures = 0;
            }
        }
    }

    private void EnsureEngineInitialized()
    {
        if (_engine != null)
            return;

        InitializeEngine();
    }

    private void InitializeEngine()
    {
        _initializationError = null;
        _logger.LogInformation(
            "[GroundingDINO] 開始初始化引擎。ModelPath={ModelPath} | VocabPath={VocabPath}",
            _config.ModelPath,
            _config.VocabPath);

        try
        {
            // 先確認檔案存在，提供更明確的錯誤訊息
            if (!File.Exists(_config.ModelPath))
            {
                _initializationError = $"找不到模型檔案：{_config.ModelPath}";
                _logger.LogError("[GroundingDINO] {Error}", _initializationError);
                return;
            }

            if (!File.Exists(_config.VocabPath))
            {
                _initializationError = $"找不到詞彙表檔案：{_config.VocabPath}";
                _logger.LogError("[GroundingDINO] {Error}", _initializationError);
                return;
            }

            var modelFileSize = new FileInfo(_config.ModelPath).Length;
            _logger.LogInformation(
                "[GroundingDINO] 模型檔案確認存在，大小 {SizeMB:F1} MB，開始載入 ONNX Session（可能需要數秒）...",
                modelFileSize / 1024.0 / 1024.0);

            _engine = new DetectionEngine(_config);
            _engineInitializedTime = DateTime.Now;
            _consecutiveFailures = 0;

            _logger.LogInformation("[GroundingDINO] 引擎初始化成功，已就緒。");
        }
        catch (Exception ex)
        {
            _engine = null;
            _engineInitializedTime = null;
            _initializationError = $"引擎初始化失敗：{ex.Message}";

            _logger.LogError(
                ex,
                "[GroundingDINO] 引擎初始化失敗。ModelPath={ModelPath} | Error={Error}",
                _config.ModelPath,
                ex.Message);
        }
    }

    private void TryRepairModelFiles()
    {
        try
        {
            _engine?.Dispose();
            _engine = null;
            _engineInitializedTime = null;

            _initializationError = "模型檔案損壞或無效，請手動檢查並重新上傳模型檔案";
            _logger.LogError("[GroundingDINO] 模型檔案損壞或無效，請手動檢查並重新上傳。ModelPath={ModelPath}", _config.ModelPath);
        }
        catch (Exception ex)
        {
            _initializationError = $"修復失敗：{ex.Message}。請手動檢查文件。";
            _logger.LogError(ex, "[GroundingDINO] 修復模型文件時發生異常。");
        }
    }

    private void StartModelDownload()
    {
        if (_isDownloading)
        {
            _initializationError = "模型正在下載中，請稍後再試...";
            return;
        }

        _initializationError = "模型檔案不存在，系統正在自動下載中，請稍候...";

        Task.Run(async () =>
        {
            try
            {
                var (success, message) = await DownloadModelsAsync();

                if (!success)
                {
                    lock (_lock)
                    {
                        _initializationError = $"模型下載失敗：{message}";
                    }
                    return;
                }

                lock (_lock)
                {
                    _initializationError = null;
                    if (_engine == null)
                    {
                        InitializeEngine();
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _initializationError = $"模型下載異常：{ex.Message}";
                }
            }
        });
    }

    private async Task<(bool success, string message)> DownloadModelsAsync()
    {
        List<(string fileName, string url, string path)> filesToDownload;

        lock (_lock)
        {
            if (_isDownloading)
                return (false, "已有下載任務正在進行中");

            _isDownloading = true;

            _fileProgress.Clear();
            _downloadingFiles.Clear();
            _downloadQueue.Clear();
        }

        filesToDownload = new List<(string fileName, string url, string path)>();
        if (!File.Exists(_config.ModelPath))
        {
            filesToDownload.Add(("groundingdino.onnx", RemoteModelUrl, _config.ModelPath));
        }
        if (!File.Exists(_config.VocabPath))
        {
            filesToDownload.Add(("vocab.txt", RemoteVocabUrl, _config.VocabPath));
        }

        if (filesToDownload.Count == 0)
        {
            lock (_lock)
            {
                _isDownloading = false;
            }
            return (true, "所有檔案已存在");
        }

        try
        {
            var modelDir = Path.GetDirectoryName(Path.GetFullPath(_config.ModelPath));
            if (!string.IsNullOrEmpty(modelDir) && !Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir);
                _logger.LogInformation("[GroundingDINO] 已建立模型目錄：{ModelDir}", modelDir);
            }

            var vocabDir = Path.GetDirectoryName(Path.GetFullPath(_config.VocabPath));
            if (!string.IsNullOrEmpty(vocabDir) && !Directory.Exists(vocabDir))
            {
                Directory.CreateDirectory(vocabDir);
                _logger.LogInformation("[GroundingDINO] 已建立詞彙表目錄：{VocabDir}", vocabDir);
            }

            var downloadTasks = new List<Task<(bool success, string message, string fileName)>>();

            foreach (var (fileName, url, path) in filesToDownload)
            {
                lock (_lock)
                {
                    _downloadingFiles.Add(fileName);
                    _fileProgress[fileName] = (0, null);
                }

                var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
                var task = DownloadFileAsyncWithProgress(httpClient, url, path, fileName)
                    .ContinueWith<(bool success, string message, string fileName)>(t =>
                    {
                        httpClient.Dispose();
                        if (t.IsFaulted)
                            return (false, t.Exception?.InnerException?.Message ?? "下載異常", fileName);
                        return (t.Result.success, t.Result.message, fileName);
                    });

                downloadTasks.Add(task);
            }

            var results = await Task.WhenAll(downloadTasks);

            var failedFiles = results.Where(r => !r.success).ToList();
            if (failedFiles.Any())
            {
                lock (_lock)
                {
                    _isDownloading = false;
                    _downloadingFiles.Clear();
                    _fileProgress.Clear();
                }
                return (false, $"下載失敗：{string.Join(", ", failedFiles.Select(f => $"{f.fileName} ({f.message})"))}");
            }

            lock (_lock)
            {
                _isDownloading = false;
                _downloadingFiles.Clear();
                _fileProgress.Clear();
            }

            return (true, "所有模型文件下載完成");
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _isDownloading = false;
                _downloadingFiles.Clear();
                _fileProgress.Clear();
            }
            return (false, $"下載失敗：{ex.Message}");
        }
    }

    private async Task<(bool success, string message)> DownloadFileAsyncWithProgress(
        HttpClient httpClient,
        string url,
        string savePath,
        string fileName)
    {
        try
        {
            var fullSavePath = Path.GetFullPath(savePath);

            var fileNameOnly = Path.GetFileName(fullSavePath);
            if (fileNameOnly != "groundingdino.onnx" && fileNameOnly != "vocab.txt")
            {
                return (false, $"{fileName} 文件名不匹配，跳過操作以保護其他文件");
            }

            var downloadDir = Path.GetDirectoryName(fullSavePath);
            if (!string.IsNullOrEmpty(downloadDir) && !Directory.Exists(downloadDir))
            {
                try
                {
                    Directory.CreateDirectory(downloadDir);
                }
                catch (Exception ex)
                {
                    return (false, $"{fileName} 無法創建目錄：{ex.Message}");
                }
            }

            if (File.Exists(fullSavePath))
            {
                try
                {
                    File.Delete(fullSavePath);
                }
                catch (Exception ex)
                {
                    return (false, $"{fileName} 無法刪除舊文件：{ex.Message}");
                }
            }

            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;

            lock (_lock)
            {
                _fileProgress[fileName] = (0, totalBytes);
            }

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fullSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;
            var lastProgressUpdate = DateTime.Now;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;

                var now = DateTime.Now;
                if ((now - lastProgressUpdate).TotalMilliseconds >= 100 || downloadedBytes % (1024 * 1024) == 0)
                {
                    lock (_lock)
                    {
                        _fileProgress[fileName] = (downloadedBytes, totalBytes);
                    }
                    lastProgressUpdate = now;
                }
            }

            lock (_lock)
            {
                _fileProgress[fileName] = (downloadedBytes, totalBytes);
                if (_downloadingFiles.Contains(fileName))
                    _downloadingFiles.Remove(fileName);
            }

            if (totalBytes.HasValue && downloadedBytes != totalBytes.Value)
            {
                try
                {
                    if (File.Exists(fullSavePath))
                        File.Delete(fullSavePath);
                }
                catch { }
                return (false, $"{fileName} 下載不完整（期望 {totalBytes.Value} 位元組，實際 {downloadedBytes} 位元組）");
            }

            if (!File.Exists(fullSavePath))
            {
                return (false, $"{fileName} 下載後文件不存在");
            }

            var fileInfo = new FileInfo(fullSavePath);
            if (fileInfo.Length == 0)
            {
                try
                {
                    File.Delete(fullSavePath);
                }
                catch { }
                return (false, $"{fileName} 下載後文件大小為 0");
            }

            return (true, $"{fileName} 下載完成");
        }
        catch (HttpRequestException ex)
        {
            lock (_lock)
            {
                _fileProgress.Remove(fileName);
                if (_downloadingFiles.Contains(fileName))
                    _downloadingFiles.Remove(fileName);
            }

            try
            {
                if (File.Exists(savePath))
                    File.Delete(savePath);
            }
            catch { }

            return (false, $"{fileName} 下載失敗：{ex.Message}");
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _fileProgress.Remove(fileName);
                if (_downloadingFiles.Contains(fileName))
                    _downloadingFiles.Remove(fileName);
            }

            try
            {
                if (File.Exists(savePath))
                    File.Delete(savePath);
            }
            catch { }

            return (false, $"{fileName} 保存失敗：{ex.Message}");
        }
    }

    private bool AreModelFilesExists()
    {
        return File.Exists(_config.ModelPath) && File.Exists(_config.VocabPath);
    }

    private bool IsModelFileValid(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
                return false;

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] header = new byte[4];
            int bytesRead = fs.Read(header, 0, 4);

            if (bytesRead < 4)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void NotifyDetectionFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            _logger.LogWarning(
                "[GroundingDINO] 偵測失敗通知，連續失敗次數：{FailureCount}/{MaxFailures}",
                _consecutiveFailures,
                MaxConsecutiveFailures);

            if (_consecutiveFailures >= MaxConsecutiveFailures)
            {
                _logger.LogError(
                    "[GroundingDINO] 連續失敗次數達到閾值 {MaxFailures}，卸載引擎並觸發修復流程。",
                    MaxConsecutiveFailures);
                _engine?.Dispose();
                _engine = null;

                PerformHealthCheck(null);
            }
            else
            {
                _engine?.Dispose();
                _engine = null;
            }
        }
    }

    public (DateTime lastCheckTime, int consecutiveFailures) GetHealthCheckInfo()
    {
        return (_lastHealthCheckTime, _consecutiveFailures);
    }

    public void ResetFailureCount()
    {
        _consecutiveFailures = 0;
    }

    /// <summary>
    /// 手動啟動模型引擎（載入到記憶體）。
    /// 此方法不會拋出 exception，失敗原因可透過 GetDetailedStatus().InitializationError 取得。
    /// </summary>
    public void StartEngine()
    {
        lock (_lock)
        {
            if (_engine != null)
            {
                _logger.LogDebug("[GroundingDINO] StartEngine 被呼叫，引擎已在運行中，跳過。");
                return;
            }

            InitializeEngine();
        }
    }

    /// <summary>
    /// 手動停止模型引擎（從記憶體卸載）
    /// </summary>
    public void StopEngine()
    {
        lock (_lock)
        {
            if (_engine == null)
            {
                _logger.LogDebug("[GroundingDINO] StopEngine 被呼叫，但引擎未載入，跳過。");
                return;
            }

            _logger.LogInformation("[GroundingDINO] 開始手動卸載引擎...");
            _engine?.Dispose();
            _engine = null;
            _engineInitializedTime = null;
            _initializationError = null;
            _logger.LogInformation("[GroundingDINO] 引擎已卸載。");
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _healthCheckTimer?.Dispose();
            _healthCheckTimer = null;
            _engine?.Dispose();
            _engine = null;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

