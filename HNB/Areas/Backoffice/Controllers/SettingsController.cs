using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class SettingsController(SettingsServices svc) : BaseController
{
    public IActionResult Settings()
    {
        svc.ViewBagModel(ViewBag);
        return View();
    }

    /// <summary>
    /// 清理日誌
    /// </summary>
    [HttpPost]
    public IActionResult ClearLogs(string logType, bool is30Days = false)
    {
        bool success;
        string message;
        
        switch (logType.ToLower())
        {
            case "error":
                success = svc.DeleteErrorLogs(is30Days);
                message = success ? $"錯誤日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "錯誤日誌清理失敗";
                break;
            case "access":
                success = svc.DeleteAccessRecords(is30Days);
                message = success ? $"存取記錄清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "存取記錄清理失敗";
                break;
            case "system":
                success = svc.DeleteAccessRecords(is30Days);
                message = success ? $"系統日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "系統日誌清理失敗";
                break;
            default:
                success = false;
                message = "不支援的日誌類型";
                break;
        }

        return Json(new { success, message });
    }

    /// <summary>
    /// 清理快取
    /// </summary>
    [HttpPost]
    public IActionResult ClearCache(string cacheType, bool is30Days = false)
    {
        bool success;
        string message;
        
        switch (cacheType.ToLower())
        {
            case "memory":
            case "database":
            case "file":
            case "all":
                success = svc.ClearAllCache();
                message = success ? "快取清理成功" : "快取清理失敗";
                break;
            default:
                success = false;
                message = "不支援的快取類型";
                break;
        }

        return Json(new { success, message });
    }

    /// <summary>
    /// 刷新日誌統計
    /// </summary>
    public IActionResult RefreshLogStats()
    {
        var errorLogs = svc.LoadErrorLogsCount();
        var accessLogs = svc.LoadAccessLogsCount();
        return Json(new { 
            success = true, 
            errorLogs = errorLogs,
            accessLogs = accessLogs
        });
    }

    /// <summary>
    /// 刷新快取統計
    /// </summary>
    public IActionResult RefreshCacheStats()
    {
        var cacheStats = svc.LoadCacheStatistics();
        return Json(new { 
            success = true, 
            memoryCacheSize = cacheStats.memoryCacheSize,
            cacheEntries = cacheStats.cacheEntries,
            lastCleared = cacheStats.lastCleared.ToString("yyyy-MM-dd HH:mm")
        });
    }

    /// <summary>
    /// 切換維護模式
    /// </summary>
    [HttpPost]
    public IActionResult ToggleMaintenanceMode(bool enabled)
    {
        var result = svc.CreateMaintenanceMode(enabled);
        var success = result != null;
        var message = success 
            ? (enabled ? "維護模式已啟用" : "維護模式已停用")
            : "切換維護模式失敗";
        
        return Json(new { success, message });
    }

    /// <summary>
    /// 系統健康檢查（直接調用資料庫資料）
    /// </summary>
    [HttpGet]
    public IActionResult SystemHealth()
    {
        var healthInfo = svc.LoadSystemHealth();
        return Json(new { success = true, data = healthInfo });
    }

    /// <summary>
    /// 匯出日誌
    /// </summary>
    [HttpGet]
    public IActionResult ExportLogs(string logType = "all")
    {
        var logData = svc.LoadExportLogs(logType);
        var fileName = $"logs_{logType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        
        return Json(new { 
            success = true, 
            data = logData,
            fileName = fileName,
            message = "日誌匯出成功"
        });
    }

    /// <summary>
    /// 優化資料庫
    /// </summary>
    [HttpPost]
    public IActionResult OptimizeDatabase()
    {
        var result = svc.LoadDatabaseOptimization();
        return Json(new { 
            success = result.success, 
            message = result.message,
            details = result.details
        });
    }

    /// <summary>
    /// 重啟系統
    /// </summary>
    [HttpPost]
    public IActionResult RestartSystem()
    {
        var result = svc.LoadSystemRestart();
        return Json(new { 
            success = result.success, 
            message = result.message
        });
    }
}
