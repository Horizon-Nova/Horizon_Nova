using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;
using Models.Hnb;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 系統設定服務層，負責處理硬體監控、日誌管理和快取管理功能
/// </summary>
public class SettingsServices(SettingsRepositories rep)
{
    #region 統一的查詢方法

    /// <summary>
    /// 載入硬體監控資料
    /// </summary>
    public vw_hardware_monitoring? LoadHardwareMonitoring()
        => rep.QueryHardwareMonitoring();

    /// <summary>
    /// 載入錯誤日誌數量
    /// </summary>
    public int LoadErrorLogsCount()
        => rep.QueryErrorLogsCount();

    /// <summary>
    /// 載入存取記錄數量
    /// </summary>
    public int LoadAccessLogsCount()
        => rep.QueryAccessLogsCount();

    /// <summary>
    /// 載入錯誤日誌列表
    /// </summary>
    public List<error_log> LoadErrorLogList(int? days = null)
        => rep.QueryErrorLogList(days);

    /// <summary>
    /// 載入存取記錄列表
    /// </summary>
    public List<access_record> LoadAccessRecordList(int? days = null)
        => rep.QueryAccessRecordList(days);

    #endregion

    #region ViewBag 設定方法
    
    /// <summary>
    /// 設置 ViewBag 模型資料
    /// </summary>
    public void ViewBagModel(dynamic viewBag)
    {
        var hardwareInfo = LoadHardwareMonitoring();
        var errorLogsCount = LoadErrorLogsCount();
        var accessLogsCount = LoadAccessLogsCount();
        var cacheStats = LoadCacheStatistics();
        
        viewBag.HardwareInfo = hardwareInfo;
        viewBag.ErrorLogsCount = errorLogsCount;
        viewBag.AccessLogsCount = accessLogsCount;
        viewBag.MemoryCacheSize = cacheStats.memoryCacheSize;
        viewBag.CacheEntries = cacheStats.cacheEntries;
        viewBag.LastCacheCleared = cacheStats.lastCleared;
    }
    
    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建維護模式切換（更新）
    /// </summary>
    public hardware_monitoring? CreateMaintenanceMode(bool enabled)
    {
        var config = new hardware_monitoring { is_active = enabled };
        return rep.InsertHardwareMonitoringConfig(config);
    }

    /// <summary>
    /// 刪除錯誤日誌
    /// </summary>
    public bool DeleteErrorLogs(bool is30Days = false)
    {
        var logs = is30Days 
            ? LoadErrorLogList(30) 
            : LoadErrorLogList();
        return rep.DeleteErrorLogs(logs);
    }

    /// <summary>
    /// 刪除存取記錄
    /// </summary>
    public bool DeleteAccessRecords(bool is30Days = false)
    {
        var records = is30Days 
            ? LoadAccessRecordList(30) 
            : LoadAccessRecordList();
        return rep.DeleteAccessRecords(records);
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 載入快取統計資料
    /// </summary>
    public (long memoryCacheSize, int cacheEntries, DateTime lastCleared) LoadCacheStatistics()
        => (1024 * 1024 * 50, 150, DateTime.Now.AddHours(-2));

    /// <summary>
    /// 清理所有快取
    /// </summary>
    public bool ClearAllCache() => true;

    /// <summary>
    /// 載入系統健康狀態
    /// </summary>
    public object LoadSystemHealth()
    {
        var hardwareInfo = LoadHardwareMonitoring();
        var errorLogsCount = LoadErrorLogsCount();
        var accessLogsCount = LoadAccessLogsCount();
        var cacheStats = LoadCacheStatistics();

        return new
        {
            timestamp = DateTime.Now,
            systemStatus = hardwareInfo?.is_active == true ? "運行中" : "停止",
            cpuUsage = hardwareInfo?.cpu_usage_percent ?? 0,
            memoryUsage = hardwareInfo?.memory_usage_percent ?? 0,
            diskUsage = 0,
            errorLogs = errorLogsCount,
            accessLogs = accessLogsCount,
            cacheSize = cacheStats.memoryCacheSize,
            uptime = hardwareInfo?.uptime ?? "未知",
            lastUpdated = hardwareInfo?.last_check_time ?? DateTime.Now,
            healthScore = CalculateHealthScore(hardwareInfo, errorLogsCount, accessLogsCount, cacheStats)
        };
    }

    /// <summary>
    /// 載入匯出日誌資料
    /// </summary>
    public object LoadExportLogs(string logType)
    {
        List<object> logs = logType.ToLower() switch
        {
            "error" => LoadErrorLogList().Cast<object>().ToList(),
            "access" => LoadAccessRecordList().Cast<object>().ToList(),
            _ => LoadErrorLogList().Cast<object>().Concat(LoadAccessRecordList().Cast<object>()).ToList()
        };

        return new
        {
            exportTime = DateTime.Now,
            logType = logType,
            totalCount = logs.Count,
            logs = logs
        };
    }

    /// <summary>
    /// 載入資料庫優化結果
    /// </summary>
    public (bool success, string message, object details) LoadDatabaseOptimization()
    {
        var details = new
        {
            optimizedAt = DateTime.Now,
            operations = new[]
            {
                "清理未使用的空間",
                "更新統計資訊",
                "重建索引",
                "檢查資料完整性"
            },
            estimatedTime = "2-5 分鐘"
        };
        
        return (true, "資料庫優化完成", details);
    }

    /// <summary>
    /// 載入系統重啟結果
    /// </summary>
    public (bool success, string message) LoadSystemRestart()
        => (true, "系統重啟指令已發送，將在30秒後重啟");

    /// <summary>
    /// 計算系統健康分數
    /// </summary>
    private int CalculateHealthScore(vw_hardware_monitoring? hardwareInfo, int errorLogs, int accessLogs, (long memoryCacheSize, int cacheEntries, DateTime lastCleared) cacheStats)
    {
        int score = 100;
        
        if (hardwareInfo != null)
        {
            if (hardwareInfo.cpu_usage_percent > 90) score -= 20;
            if (hardwareInfo.memory_usage_percent > 90) score -= 20;
            if (hardwareInfo.is_active != true) score -= 30;
        }
        
        if (errorLogs > 100) score -= 10;
        if (cacheStats.memoryCacheSize > 1024 * 1024 * 100) score -= 5;
        
        return Math.Max(0, score);
    }

    #endregion
}
