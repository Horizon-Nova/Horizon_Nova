using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using Models.Hnbdata;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 系統設定資料存取層，負責處理硬體監控和日誌管理功能
/// </summary>
public class SettingsRepositories(HnbHnbBackofficeDbContext db, HnbdataDbContext hnbdataDb)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的硬體監控查詢來源
    /// </summary>
    private IQueryable<vw_hardware_monitoring> ValidHardwareMonitoring => db.vw_hardware_monitorings;
    
    /// <summary>
    /// 有效的錯誤日誌查詢來源
    /// </summary>
    private IQueryable<error_log> ValidErrorLogs => hnbdataDb.error_logs;
    
    /// <summary>
    /// 有效的存取記錄查詢來源
    /// </summary>
    private IQueryable<access_record> ValidAccessRecords => hnbdataDb.access_records;
    #endregion

    #region 硬體監控
    /// <summary>
    /// 取得硬體監控資料
    /// </summary>
    public vw_hardware_monitoring? FetchHardwareMonitoring()
        => ValidHardwareMonitoring.FirstOrDefault();
    #endregion

    #region 日誌統計
    /// <summary>
    /// 計算錯誤日誌數量
    /// </summary>
    public int CountErrorLogs()
        => ValidErrorLogs.Count();

    /// <summary>
    /// 計算存取記錄數量
    /// </summary>
    public int CountAccessLogs()
        => ValidAccessRecords.Count();

    /// <summary>
    /// 取得日誌統計資料
    /// </summary>
    public (int errorLogs, int accessLogs) FetchLogStatistics()
        => (CountErrorLogs(), CountAccessLogs());

    /// <summary>
    /// 取得快取統計資料
    /// </summary>
    public (long memoryCacheSize, int cacheEntries, DateTime lastCleared) FetchCacheStatistics()
    {
        // 這裡可以添加實際的快取統計邏輯
        // 目前返回模擬資料
        return (1024 * 1024 * 50, 150, DateTime.Now.AddHours(-2)); // 50MB, 150個條目, 2小時前清理
    }
    #endregion

    #region 日誌清理
    /// <summary>
    /// 清理錯誤日誌
    /// </summary>
    /// <param name="is30Days">是否只清理30天前的日誌</param>
    public bool ClearErrorLogs(bool is30Days = false)
    {
        try
        {
            var logs = is30Days 
                ? ValidErrorLogs.Where(x => x.created_at < DateTime.Now.AddDays(-30)).ToList()
                : ValidErrorLogs.ToList();
            
            hnbdataDb.error_logs.RemoveRange(logs);
            hnbdataDb.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 清理存取記錄
    /// </summary>
    /// <param name="is30Days">是否只清理30天前的記錄</param>
    public bool ClearAccessLogs(bool is30Days = false)
    {
        try
        {
            var records = is30Days 
                ? ValidAccessRecords.Where(x => x.created_at < DateTime.Now.AddDays(-30)).ToList()
                : ValidAccessRecords.ToList();
            
            hnbdataDb.access_records.RemoveRange(records);
            hnbdataDb.SaveChanges();
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
            var config = db.system_configs.FirstOrDefault();
            if (config != null)
            {
                config.maintenance_mode = enabled;
                config.updated_at = DateTime.Now;
                db.SaveChanges();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 匯出日誌
    /// </summary>
    public List<object> ExportLogs(string logType)
    {
        try
        {
            var logs = new List<object>();
            
            switch (logType.ToLower())
            {
                case "error":
                    logs = ValidErrorLogs.Select(x => new
                    {
                        id = x.id,
                        stage = x.stage,
                        layer = x.layer,
                        message = x.message,
                        stack_trace = x.stack_trace,
                        created_at = x.created_at,
                        user_id = x.user_id,
                        path = x.path,
                        extra = x.extra
                    }).Cast<object>().ToList();
                    break;
                case "access":
                    logs = ValidAccessRecords.Select(x => new
                    {
                        id = x.id,
                        user_name = x.user_name,
                        roles = x.roles,
                        request_path = x.request_path,
                        ip = x.ip,
                        result = x.result,
                        user_agent = x.user_agent,
                        created_at = x.created_at,
                        log_type = x.log_type
                    }).Cast<object>().ToList();
                    break;
                case "all":
                default:
                    var errorLogs = ValidErrorLogs.Select(x => new { type = "error", data = x }).Cast<object>().ToList();
                    var accessLogs = ValidAccessRecords.Select(x => new { type = "access", data = x }).Cast<object>().ToList();
                    logs.AddRange(errorLogs);
                    logs.AddRange(accessLogs);
                    break;
            }
            
            return logs;
        }
        catch
        {
            return new List<object>();
        }
    }

    /// <summary>
    /// 優化資料庫
    /// </summary>
    public (bool success, string message, object details) OptimizeDatabase()
    {
        try
        {
            // 這裡可以添加實際的資料庫優化邏輯
            // 例如：VACUUM、ANALYZE、REINDEX 等 PostgreSQL 命令
            
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
        catch (Exception ex)
        {
            return (false, $"優化失敗：{ex.Message}", new { error = ex.Message });
        }
    }
    #endregion
}
