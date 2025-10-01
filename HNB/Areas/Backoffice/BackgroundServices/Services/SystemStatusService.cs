using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.HnbHnbBackoffice;
using System.Diagnostics;
using System.Management;

namespace HNB.Areas.Backoffice.BackgroundServices.Services;

/// <summary>
/// 系統狀態監控背景服務
/// 定期檢查系統狀態並更新到資料庫
/// </summary>
public class SystemStatusService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SystemStatusService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(2); // 每2分鐘執行一次

    public SystemStatusService(
        IServiceProvider serviceProvider,
        ILogger<SystemStatusService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("系統狀態監控服務已啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorSystemStatus();
                _logger.LogDebug("系統狀態檢查完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "系統狀態監控服務執行時發生錯誤");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("系統狀態監控服務已停止");
    }

    /// <summary>
    /// 監控系統狀態
    /// </summary>
    private async Task MonitorSystemStatus()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HnbHnbBackofficeDbContext>();

        try
        {
            // 取得系統配置
            var systemConfig = dbContext.system_configs.FirstOrDefault();
            if (systemConfig == null)
            {
                _logger.LogWarning("找不到系統配置，跳過狀態監控");
                return;
            }

            // 收集系統狀態
            var systemStatus = await CollectSystemStatus();

            // 更新系統配置
            await UpdateSystemStatus(dbContext, systemConfig, systemStatus);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "監控系統狀態時發生錯誤");
        }
    }

    /// <summary>
    /// 收集系統狀態
    /// </summary>
    private async Task<SystemStatusInfo> CollectSystemStatus()
    {
        var status = new SystemStatusInfo
        {
            Timestamp = DateTime.UtcNow,
            SystemStatus = "運行中",
            CpuUsage = await GetCpuUsage(),
            MemoryUsage = await GetMemoryUsage(),
            DiskUsage = await GetDiskUsage(),
            NetworkStatus = await GetNetworkStatus(),
            ServiceStatus = await GetServiceStatus(),
            ProcessCount = Process.GetProcesses().Length,
            Uptime = GetSystemUptime(),
            LastActivity = DateTime.UtcNow
        };

        // 檢查系統健康狀態
        status.HealthScore = CalculateHealthScore(status);
        status.SystemStatus = status.HealthScore > 80 ? "運行中" : status.HealthScore > 50 ? "警告" : "錯誤";

        return status;
    }

    /// <summary>
    /// 取得 CPU 使用率
    /// </summary>
    private async Task<decimal> GetCpuUsage()
    {
        try
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // 第一次調用返回 0
            await Task.Delay(1000); // 等待 1 秒
            return (decimal)cpuCounter.NextValue();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 CPU 使用率時發生錯誤");
        }

        return 0;
    }

    /// <summary>
    /// 取得記憶體使用率
    /// </summary>
    private async Task<decimal> GetMemoryUsage()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                var freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                var usedMemory = totalMemory - freeMemory;

                if (totalMemory > 0)
                {
                    return (decimal)(usedMemory * 100) / totalMemory;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得記憶體使用率時發生錯誤");
        }

        return 0;
    }

    /// <summary>
    /// 取得磁碟使用率
    /// </summary>
    private async Task<decimal> GetDiskUsage()
    {
        try
        {
            var drives = DriveInfo.GetDrives();
            decimal totalUsage = 0;
            int driveCount = 0;

            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    var totalSize = drive.TotalSize;
                    var freeSpace = drive.TotalFreeSpace;
                    var usedSpace = totalSize - freeSpace;

                    if (totalSize > 0)
                    {
                        totalUsage += (decimal)(usedSpace * 100) / totalSize;
                        driveCount++;
                    }
                }
            }

            return driveCount > 0 ? totalUsage / driveCount : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得磁碟使用率時發生錯誤");
        }

        return 0;
    }

    /// <summary>
    /// 取得網路狀態
    /// </summary>
    private async Task<string> GetNetworkStatus()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
            var collection = searcher.Get();

            var activeConnections = 0;
            foreach (ManagementObject obj in collection)
            {
                activeConnections++;
            }

            return activeConnections > 0 ? "已連接" : "未連接";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得網路狀態時發生錯誤");
        }

        return "未知";
    }

    /// <summary>
    /// 取得服務狀態
    /// </summary>
    private async Task<string> GetServiceStatus()
    {
        try
        {
            var criticalServices = new[] { "Spooler", "RpcSs", "LanmanServer", "LanmanWorkstation" };
            var runningServices = 0;

            foreach (var serviceName in criticalServices)
            {
                try
                {
                    using var service = new System.ServiceProcess.ServiceController(serviceName);
                    if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        runningServices++;
                    }
                }
                catch
                {
                    // 服務不存在或無法訪問
                }
            }

            var percentage = (runningServices * 100) / criticalServices.Length;
            return percentage > 75 ? "正常" : percentage > 50 ? "警告" : "錯誤";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得服務狀態時發生錯誤");
        }

        return "未知";
    }

    /// <summary>
    /// 取得系統運行時間
    /// </summary>
    private string GetSystemUptime()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}天 {uptime.Hours}小時 {uptime.Minutes}分鐘";
        }
        catch
        {
            return "未知";
        }
    }

    /// <summary>
    /// 計算健康分數
    /// </summary>
    private int CalculateHealthScore(SystemStatusInfo status)
    {
        int score = 100;

        // CPU 使用率評分
        if (status.CpuUsage > 90) score -= 30;
        else if (status.CpuUsage > 70) score -= 15;

        // 記憶體使用率評分
        if (status.MemoryUsage > 90) score -= 25;
        else if (status.MemoryUsage > 70) score -= 10;

        // 磁碟使用率評分
        if (status.DiskUsage > 90) score -= 20;
        else if (status.DiskUsage > 80) score -= 10;

        // 網路狀態評分
        if (status.NetworkStatus != "已連接") score -= 15;

        // 服務狀態評分
        if (status.ServiceStatus == "錯誤") score -= 20;
        else if (status.ServiceStatus == "警告") score -= 10;

        // 程序數量評分（過多程序可能表示系統負載過重）
        if (status.ProcessCount > 200) score -= 10;
        else if (status.ProcessCount > 150) score -= 5;

        return Math.Max(0, score);
    }

    /// <summary>
    /// 更新系統狀態
    /// </summary>
    private async Task UpdateSystemStatus(HnbHnbBackofficeDbContext dbContext, system_config config, SystemStatusInfo status)
    {
        config.system_status = status.SystemStatus;
        config.last_updated = status.Timestamp;
        config.uptime = status.Uptime;
        config.updated_at = DateTime.UtcNow;

        // 更新最近活動
        config.last_activity_type = "系統狀態更新";
        config.last_activity_description = $"系統健康分數: {status.HealthScore}/100, CPU: {status.CpuUsage:F1}%, 記憶體: {status.MemoryUsage:F1}%";
        config.last_activity_timestamp = DateTime.UtcNow;

        // 更新最近活動 JSON
        var recentActivities = new List<object>
        {
            new
            {
                type = "系統狀態更新",
                description = $"健康分數: {status.HealthScore}/100",
                timestamp = DateTime.UtcNow,
                details = new
                {
                    cpu_usage = status.CpuUsage,
                    memory_usage = status.MemoryUsage,
                    disk_usage = status.DiskUsage,
                    network_status = status.NetworkStatus,
                    service_status = status.ServiceStatus,
                    process_count = status.ProcessCount
                }
            }
        };

        config.recent_activities = System.Text.Json.JsonSerializer.Serialize(recentActivities);
    }

    /// <summary>
    /// 系統狀態資訊
    /// </summary>
    public class SystemStatusInfo
    {
        public DateTime Timestamp { get; set; }
        public string SystemStatus { get; set; } = "未知";
        public decimal CpuUsage { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal DiskUsage { get; set; }
        public string NetworkStatus { get; set; } = "未知";
        public string ServiceStatus { get; set; } = "未知";
        public int ProcessCount { get; set; }
        public string Uptime { get; set; } = "未知";
        public DateTime LastActivity { get; set; }
        public int HealthScore { get; set; }
    }
}
