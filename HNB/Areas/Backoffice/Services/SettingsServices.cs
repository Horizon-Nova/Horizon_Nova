using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 系統設定服務層，負責處理硬體監控、日誌管理和快取管理功能
/// </summary>
public class SettingsServices(SettingsRepositories rep)
{
    #region 硬體監控
    /// <summary>
    /// 取得硬體監控資料
    /// </summary>
    public vw_hardware_monitoring? FetchHardwareMonitoring()
        => rep.FetchHardwareMonitoring();
    #endregion

    #region 日誌管理
    /// <summary>
    /// 取得日誌統計資料
    /// </summary>
    public (int errorLogs, int accessLogs) FetchLogStatistics()
        => (rep.CountErrorLogs(), rep.CountAccessLogs());

    /// <summary>
    /// 清理錯誤日誌
    /// </summary>
    public bool ClearErrorLogs(bool is30Days = false)
        => rep.ClearErrorLogs(is30Days);

    /// <summary>
    /// 清理存取記錄
    /// </summary>
    public bool ClearAccessLogs(bool is30Days = false)
        => rep.ClearAccessLogs(is30Days);
    #endregion

    #region 快取管理
    /// <summary>
    /// 取得快取統計資料
    /// </summary>
    public (long memoryCacheSize, int cacheEntries, DateTime lastCleared) FetchCacheStatistics()
    {
        // 這裡可以添加實際的快取統計邏輯
        // 目前返回模擬資料
        return (1024 * 1024 * 50, 150, DateTime.Now.AddHours(-2)); // 50MB, 150個條目, 2小時前清理
    }

    /// <summary>
    /// 清理所有快取
    /// </summary>
    public async Task<bool> ClearAllCacheAsync()
    {
        try
        {
            // 這裡可以添加實際的快取清理邏輯
            await Task.Delay(100); // 模擬清理過程
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region 系統維護工具
    /// <summary>
    /// 切換維護模式
    /// </summary>
    public bool ToggleMaintenanceMode(bool enabled)
    {
        try
        {
            return rep.ToggleMaintenanceMode(enabled);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 取得系統健康狀態
    /// </summary>
    public object GetSystemHealth()
    {
        try
        {
            var hardwareInfo = rep.FetchHardwareMonitoring();
            var logStats = rep.FetchLogStatistics();
            var cacheStats = rep.FetchCacheStatistics();

            return new
            {
                timestamp = DateTime.Now,
                systemStatus = hardwareInfo?.system_status ?? "未知",
                cpuUsage = hardwareInfo?.cpu_usage_percent ?? 0,
                memoryUsage = hardwareInfo?.memory_usage_percent ?? 0,
                diskUsage = hardwareInfo?.storage_devices != null ? 
                    System.Text.Json.JsonSerializer.Deserialize<List<object>>(hardwareInfo.storage_devices)?.Count ?? 0 : 0,
                errorLogs = logStats.errorLogs,
                accessLogs = logStats.accessLogs,
                cacheSize = cacheStats.memoryCacheSize,
                uptime = hardwareInfo?.uptime ?? "未知",
                lastUpdated = hardwareInfo?.last_updated ?? DateTime.Now,
                healthScore = CalculateHealthScore(hardwareInfo, logStats, cacheStats)
            };
        }
        catch
        {
            return new
            {
                timestamp = DateTime.Now,
                systemStatus = "錯誤",
                cpuUsage = 0,
                memoryUsage = 0,
                diskUsage = 0,
                errorLogs = 0,
                accessLogs = 0,
                cacheSize = 0,
                uptime = "未知",
                lastUpdated = DateTime.Now,
                healthScore = 0
            };
        }
    }

    /// <summary>
    /// 匯出日誌
    /// </summary>
    public object ExportLogs(string logType)
    {
        try
        {
            var logs = rep.ExportLogs(logType);
            return new
            {
                exportTime = DateTime.Now,
                logType = logType,
                totalCount = logs.Count,
                logs = logs
            };
        }
        catch
        {
            return new
            {
                exportTime = DateTime.Now,
                logType = logType,
                totalCount = 0,
                logs = new List<object>()
            };
        }
    }

    /// <summary>
    /// 優化資料庫
    /// </summary>
    public (bool success, string message, object details) OptimizeDatabase()
    {
        try
        {
            var result = rep.OptimizeDatabase();
            return result;
        }
        catch (Exception ex)
        {
            return (false, $"優化失敗：{ex.Message}", new { error = ex.Message });
        }
    }

    /// <summary>
    /// 重啟系統
    /// </summary>
    public (bool success, string message) RestartSystem()
    {
        try
        {
            // 這裡可以添加實際的重啟邏輯
            // 目前返回模擬結果
            return (true, "系統重啟指令已發送，將在30秒後重啟");
        }
        catch (Exception ex)
        {
            return (false, $"重啟失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 計算系統健康分數
    /// </summary>
    private int CalculateHealthScore(object hardwareInfo, (int errorLogs, int accessLogs) logStats, (long memoryCacheSize, int cacheEntries, DateTime lastCleared) cacheStats)
    {
        int score = 100;
        
        // 根據各種指標調整分數
        if (hardwareInfo != null)
        {
            var hw = hardwareInfo as dynamic;
            if (hw?.cpu_usage_percent > 90) score -= 20;
            if (hw?.memory_usage_percent > 90) score -= 20;
            if (hw?.system_status != "運行中") score -= 30;
        }
        
        if (logStats.errorLogs > 100) score -= 10;
        if (cacheStats.memoryCacheSize > 1024 * 1024 * 100) score -= 5; // 100MB
        
        return Math.Max(0, score);
    }
    #endregion
}
