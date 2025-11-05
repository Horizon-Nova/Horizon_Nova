using HNB.IntelligentSystems.ObjectDetection.Configuration;

namespace HNB.IntelligentSystems.ObjectDetection.Core;

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
    private readonly ObjectDetectionConfig _config;
    private string? _initializationError;
    private bool _isDownloading = false;

    // 下載進度追蹤（支持並行下載）
    private Dictionary<string, (long downloaded, long? total)> _fileProgress = new Dictionary<string, (long, long?)>();
    private List<string> _downloadingFiles = new List<string>();
    private List<string> _downloadQueue = new List<string>();

    private const int MaxConsecutiveFailures = 3; // 連續失敗次數閾值
    private const int HealthCheckIntervalMinutes = 5; // 健康檢查間隔（分鐘）

    // 遠程模型 URL
    private const string RemoteModelUrl = "https://horizon-nova.up.railway.app/storage/AI/groundingdino.onnx";
    private const string RemoteVocabUrl = "https://horizon-nova.up.railway.app/storage/AI/vocab.txt";

    /// <summary>
    /// 構造函數
    /// </summary>
    public ModelHealthChecker(ObjectDetectionConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // 記錄初始時間
        _lastHealthCheckTime = DateTime.Now;

        // 應用啟動時立即檢查並初始化
        EnsureEngineInitialized();

        // 啟動定期健康檢查
        StartHealthCheckTimer();
    }

    /// <summary>
    /// 取得檢測引擎（如果已初始化）
    /// </summary>
    public DetectionEngine? GetEngine()
    {
        lock (_lock)
        {
            return _engine;
        }
    }

    /// <summary>
    /// AI 模型詳細狀態資訊
    /// </summary>
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

    /// <summary>
    /// 取得模型狀態（簡化版，向後兼容）
    /// </summary>
    public (bool IsReady, string Message) GetStatus()
    {
        var info = GetDetailedStatus();
        return (info.IsReady, info.Message);
    }

    /// <summary>
    /// 取得 AI 模型詳細狀態資訊
    /// </summary>
    public ModelStatusInfo GetDetailedStatus()
    {
        lock (_lock)
        {
            // 計算總體進度
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
                EngineInitializedTime = _engineInitializedTime
            };

            // 檢查文件存在性與大小
            if (File.Exists(_config.ModelPath))
            {
                info.ModelFileExists = true;
                var modelInfo = new FileInfo(_config.ModelPath);
                info.ModelFileSize = modelInfo.Length;
            }

            if (File.Exists(_config.VocabPath))
            {
                info.VocabFileExists = true;
                var vocabInfo = new FileInfo(_config.VocabPath);
                info.VocabFileSize = vocabInfo.Length;
            }

            // 設定訊息（包含下載進度資訊）
            if (_engine != null)
            {
                info.Message = "模型已準備就緒";
                info.EngineInitializedTime = _engineInitializedTime;
            }
            else if (_isDownloading && _downloadingFiles.Count > 0)
            {
                // 正在下載時，顯示詳細進度資訊（支持多檔案並行下載）
                var filesText = _downloadingFiles.Count == 1
                    ? _downloadingFiles[0]
                    : string.Join("、", _downloadingFiles);

                var progressText = totalSize.HasValue
                    ? $"{overallProgress:F1}% ({FormatBytes(totalDownloaded)} / {FormatBytes(totalSize.Value)})"
                    : $"{FormatBytes(totalDownloaded)}";

                var queueText = _downloadQueue.Count > 0
                    ? $" (等待下載: {string.Join(", ", _downloadQueue)})"
                    : "";

                var details = new List<string>();
                foreach (var file in _downloadingFiles)
                {
                    if (fileProgress.TryGetValue(file, out var progress))
                    {
                        var fileProgressText = progress.total.HasValue
                            ? $"{progress.progress:F1}%"
                            : FormatBytes(progress.downloaded);
                        details.Add($"{file} ({fileProgressText})");
                    }
                }

                var detailsText = details.Count > 0 ? $" [{string.Join(", ", details)}]" : "";

                info.Message = $"正在並行下載 {filesText}... 總進度 {progressText}{detailsText}{queueText}";
            }
            else if (!string.IsNullOrEmpty(_initializationError))
            {
                info.Message = _initializationError;
            }
            else if (!info.ModelFileExists && !info.VocabFileExists)
            {
                info.Message = _isDownloading
                    ? "模型檔案和詞彙表檔案不存在，系統正在自動下載中..."
                    : "模型檔案和詞彙表檔案不存在，等待下載...";
            }
            else if (!info.ModelFileExists)
            {
                info.Message = _isDownloading
                    ? "模型檔案不存在，系統正在自動下載中..."
                    : "模型檔案不存在，等待下載...";
            }
            else if (!info.VocabFileExists)
            {
                info.Message = _isDownloading
                    ? "詞彙表檔案不存在，系統正在自動下載中..."
                    : "詞彙表檔案不存在，等待下載...";
            }
            else
            {
                info.Message = "模型檔案存在但尚未初始化";
            }

            return info;
        }
    }

    /// <summary>
    /// 檢查是否正在下載
    /// </summary>
    public bool IsDownloading()
    {
        lock (_lock)
        {
            return _isDownloading;
        }
    }

    /// <summary>
    /// 啟動定期健康檢查計時器
    /// </summary>
    private void StartHealthCheckTimer()
    {
        // 每 5 分鐘執行一次健康檢查
        _healthCheckTimer = new Timer(PerformHealthCheck, null,
            TimeSpan.FromMinutes(HealthCheckIntervalMinutes),
            TimeSpan.FromMinutes(HealthCheckIntervalMinutes));
    }

    /// <summary>
    /// 執行健康檢查
    /// </summary>
    private void PerformHealthCheck(object? state)
    {
        try
        {
            _lastHealthCheckTime = DateTime.Now;

            lock (_lock)
            {
                // 檢查模型文件是否存在且有效
                if (!AreModelFilesExists())
                {
                    _consecutiveFailures++;
                    if (_consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        TryRepairModelFiles();
                        _consecutiveFailures = 0;
                    }
                    else
                    {
                        EnsureEngineInitialized();
                    }
                    return;
                }

                // 驗證模型文件是否有效
                if (!IsModelFileValid(_config.ModelPath))
                {
                    _consecutiveFailures++;
                    if (_consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        TryRepairModelFiles();
                        _consecutiveFailures = 0;
                    }
                    return;
                }

                // 檢查引擎是否已初始化且有效
                if (_engine == null)
                {
                    InitializeEngineIfFilesExist();
                    return;
                }

                // 健康檢查通過，重置失敗計數
                _consecutiveFailures = 0;
            }
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不中斷健康檢查
            _consecutiveFailures++;
            _initializationError = $"健康檢查異常：{ex.Message}";
            Console.WriteLine($"健康檢查異常：{ex.Message}");
        }
    }

    /// <summary>
    /// 確保引擎已初始化
    /// </summary>
    private void EnsureEngineInitialized()
    {
        if (_engine != null)
            return;

        if (!AreModelFilesExists())
        {
            StartModelDownload();
            return;
        }

        InitializeEngineIfFilesExist();
    }

    /// <summary>
    /// 初始化引擎（如果文件存在且有效）
    /// </summary>
    private void InitializeEngineIfFilesExist()
    {
        if (!AreModelFilesExists())
            return;

        // 驗證模型文件是否有效
        if (!IsModelFileValid(_config.ModelPath))
        {
            _initializationError = "模型文件損壞或無效，自動修復中...";
            TryRepairModelFiles();
            return;
        }

        try
        {
            _initializationError = null;
            _engine = new DetectionEngine(_config);
            _engineInitializedTime = DateTime.Now;
            _consecutiveFailures = 0; // 重置失敗計數
        }
        catch (Exception ex)
        {
            _initializationError = $"模型初始化失敗：{ex.Message}。自動修復中...";
            TryRepairModelFiles();
        }
    }

    /// <summary>
    /// 嘗試修復模型文件（刪除並重新下載）
    /// </summary>
    private void TryRepairModelFiles()
    {
        try
        {
            // 清理現有引擎
            _engine?.Dispose();
            _engine = null;
            _engineInitializedTime = null;

            // 安全地刪除特定文件（只刪除我們管理的文件，不影響目錄中的其他文件）
            // 確保路徑是絕對路徑且指向我們配置的特定文件
            var modelPath = Path.GetFullPath(_config.ModelPath);
            var vocabPath = Path.GetFullPath(_config.VocabPath);

            // 驗證路徑確實是我們要管理的文件（防止誤刪）
            if (File.Exists(modelPath) && modelPath.EndsWith("groundingdino.onnx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Delete(modelPath);
                    Console.WriteLine($"[ModelHealthChecker] 已刪除模型文件：{modelPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelHealthChecker] 刪除模型文件失敗：{ex.Message}");
                }
            }

            if (File.Exists(vocabPath) && vocabPath.EndsWith("vocab.txt", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Delete(vocabPath);
                    Console.WriteLine($"[ModelHealthChecker] 已刪除詞彙表文件：{vocabPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelHealthChecker] 刪除詞彙表文件失敗：{ex.Message}");
                }
            }

            // 觸發重新下載
            StartModelDownload();
        }
        catch (Exception ex)
        {
            _initializationError = $"修復失敗：{ex.Message}。請手動檢查文件。";
            Console.WriteLine($"[ModelHealthChecker] 修復模型文件時發生異常：{ex.Message}");
        }
    }

    /// <summary>
    /// 啟動模型下載任務
    /// </summary>
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
                    // 下載完成後，嘗試初始化引擎
                    InitializeEngineIfFilesExist();
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

    /// <summary>
    /// 下載遠程模型文件（異步，並行下載）
    /// </summary>
    private async Task<(bool success, string message)> DownloadModelsAsync()
    {
        List<(string fileName, string url, string path)> filesToDownload;

        lock (_lock)
        {
            if (_isDownloading)
                return (false, "已有下載任務正在進行中");

            _isDownloading = true;

            // 初始化下載狀態
            _fileProgress.Clear();
            _downloadingFiles.Clear();
            _downloadQueue.Clear();
        }

        // 建立需要下載的檔案列表（在 lock 外執行，避免長時間鎖定）
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
            // 確保目錄存在
            var modelDir = Path.GetDirectoryName(Path.GetFullPath(_config.ModelPath));
            if (!string.IsNullOrEmpty(modelDir) && !Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir);
                Console.WriteLine($"[ModelHealthChecker] 已創建模型目錄：{modelDir}");
            }

            var vocabDir = Path.GetDirectoryName(Path.GetFullPath(_config.VocabPath));
            if (!string.IsNullOrEmpty(vocabDir) && !Directory.Exists(vocabDir))
            {
                Directory.CreateDirectory(vocabDir);
                Console.WriteLine($"[ModelHealthChecker] 已創建詞彙表目錄：{vocabDir}");
            }

            // 建立下載任務列表（並行下載）
            var downloadTasks = new List<Task<(bool success, string message, string fileName)>>();

            // 為每個檔案創建獨立的 HttpClient（避免並行下載衝突）
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

            // 等待所有下載完成
            var results = await Task.WhenAll(downloadTasks);

            // 檢查結果
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

    /// <summary>
    /// 下載單個文件（帶進度追蹤，用於並行下載）
    /// </summary>
    private async Task<(bool success, string message)> DownloadFileAsyncWithProgress(
        HttpClient httpClient,
        string url,
        string savePath,
        string fileName)
    {
        try
        {
            // 使用完整路徑，確保安全
            var fullSavePath = Path.GetFullPath(savePath);

            // 驗證文件名，確保是我們管理的文件（防止誤刪其他文件）
            var fileNameOnly = Path.GetFileName(fullSavePath);
            if (fileNameOnly != "groundingdino.onnx" && fileNameOnly != "vocab.txt")
            {
                return (false, $"{fileName} 文件名不匹配，跳過操作以保護其他文件");
            }

            // 確保目錄存在
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

            // 如果文件已存在，先刪除
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

            // 初始化這個檔案的進度
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

                // 更新這個檔案的進度（每 100ms 或每 1MB 更新一次）
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

            // 下載完成，更新最終狀態
            lock (_lock)
            {
                _fileProgress[fileName] = (downloadedBytes, totalBytes);
                if (_downloadingFiles.Contains(fileName))
                    _downloadingFiles.Remove(fileName);
            }

            // 驗證下載的文件大小
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

            // 驗證文件是否存在且大小正確
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

    /// <summary>
    /// 下載單個文件（舊方法，保留向後兼容）
    /// </summary>
    private async Task<(bool success, string message)> DownloadFileAsync(
        HttpClient httpClient,
        string url,
        string savePath,
        string fileName)
    {
        try
        {
            // 使用完整路徑，確保安全
            var fullSavePath = Path.GetFullPath(savePath);

            // 驗證文件名，確保是我們管理的文件（防止誤刪其他文件）
            var fileNameOnly = Path.GetFileName(fullSavePath);
            if (fileNameOnly != "groundingdino.onnx" && fileNameOnly != "vocab.txt")
            {
                return (false, $"{fileName} 文件名不匹配，跳過操作以保護其他文件");
            }

            // 確保目錄存在（再次確認，以防目錄被意外刪除）
            var downloadDir = Path.GetDirectoryName(fullSavePath);
            if (!string.IsNullOrEmpty(downloadDir) && !Directory.Exists(downloadDir))
            {
                try
                {
                    Directory.CreateDirectory(downloadDir);
                    Console.WriteLine($"[ModelHealthChecker] 下載前確保目錄存在：{downloadDir}");
                }
                catch (Exception ex)
                {
                    return (false, $"{fileName} 無法創建目錄：{ex.Message}");
                }
            }

            // 如果文件已存在，先刪除（確保下載新版本）
            if (File.Exists(fullSavePath))
            {
                try
                {
                    File.Delete(fullSavePath);
                    Console.WriteLine($"[ModelHealthChecker] 已刪除舊文件：{fullSavePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelHealthChecker] 無法刪除舊文件 {fullSavePath}：{ex.Message}");
                    return (false, $"{fileName} 無法刪除舊文件，請手動刪除後重試");
                }
            }

            // 使用完整路徑進行後續操作
            savePath = fullSavePath;

            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;
            var lastProgressUpdate = DateTime.Now;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;

                // 舊方法不追蹤進度（用於其他地方）
            }

            // 驗證下載的文件大小
            if (totalBytes.HasValue && downloadedBytes != totalBytes.Value)
            {
                try
                {
                    if (File.Exists(savePath))
                        File.Delete(savePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelHealthChecker] 刪除不完整文件失敗：{ex.Message}");
                }
                return (false, $"{fileName} 下載不完整（期望 {totalBytes.Value} 位元組，實際 {downloadedBytes} 位元組）");
            }

            // 驗證文件是否存在且大小正確
            if (!File.Exists(savePath))
            {
                return (false, $"{fileName} 下載後文件不存在");
            }

            var fileInfo = new FileInfo(savePath);
            if (fileInfo.Length == 0)
            {
                try
                {
                    File.Delete(savePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelHealthChecker] 刪除空文件失敗：{ex.Message}");
                }
                return (false, $"{fileName} 下載後文件大小為 0");
            }

            return (true, $"{fileName} 下載完成");
        }
        catch (HttpRequestException ex)
        {
            // 如果下載失敗，確保刪除不完整的文件
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
            // 如果保存失敗，確保刪除不完整的文件
            try
            {
                if (File.Exists(savePath))
                    File.Delete(savePath);
            }
            catch { }

            return (false, $"{fileName} 保存失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 檢查模型文件是否存在
    /// </summary>
    private bool AreModelFilesExists()
    {
        return File.Exists(_config.ModelPath) && File.Exists(_config.VocabPath);
    }

    /// <summary>
    /// 驗證模型文件是否有效
    /// </summary>
    private bool IsModelFileValid(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            // 檢查文件大小（至少應該大於 0）
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
                return false;

            // 嘗試讀取文件開頭，檢查是否為有效的文件
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

    /// <summary>
    /// 通知檢測失敗（由 ObjectDetectionModule 調用）
    /// </summary>
    public void NotifyDetectionFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            Console.WriteLine($"[ModelHealthChecker] 檢測失敗通知，連續失敗次數：{_consecutiveFailures}");

            // 只有在連續失敗達到閾值時才觸發修復，避免頻繁修復
            if (_consecutiveFailures >= MaxConsecutiveFailures)
            {
                Console.WriteLine($"[ModelHealthChecker] 連續失敗次數達到閾值 {MaxConsecutiveFailures}，觸發修復流程");
                // 清理可能有問題的引擎
                _engine?.Dispose();
                _engine = null;

                // 內部觸發健康檢查嘗試修復（僅內部使用，不對外公開）
                PerformHealthCheck(null);
            }
            else
            {
                // 僅清理引擎，不立即修復，等待定期健康檢查
                _engine?.Dispose();
                _engine = null;
            }
        }
    }

    /// <summary>
    /// 取得健康檢查資訊
    /// </summary>
    public (DateTime lastCheckTime, int consecutiveFailures) GetHealthCheckInfo()
    {
        return (_lastHealthCheckTime, _consecutiveFailures);
    }

    /// <summary>
    /// 重置失敗計數
    /// </summary>
    public void ResetFailureCount()
    {
        _consecutiveFailures = 0;
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

    /// <summary>
    /// 格式化字節數為可讀格式
    /// </summary>
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
