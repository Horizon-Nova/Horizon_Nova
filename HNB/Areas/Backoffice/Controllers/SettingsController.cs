using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class SettingsController(SettingsServices svc, SidebarNavigationService sidebarService) : BaseController(sidebarService)
{

    public IActionResult Settings()
    {
        // 設置當前頁面導航狀態
        SetActiveNavigation("/Backoffice/Settings/Settings");
        
        var hardwareInfo = svc.FetchHardwareMonitoring();
        var logStats = svc.FetchLogStatistics();
        var cacheStats = svc.FetchCacheStatistics();
        
        ViewBag.ErrorLogsCount = logStats.errorLogs;
        ViewBag.AccessLogsCount = logStats.accessLogs;
        ViewBag.MemoryCacheSize = cacheStats.memoryCacheSize;
        ViewBag.CacheEntries = cacheStats.cacheEntries;
        ViewBag.LastCacheCleared = cacheStats.lastCleared;
        
        return View(hardwareInfo);
    }

    #region AJAX 端點
    /// <summary>
    /// 清理日誌
    /// </summary>
    [HttpPost]
    public IActionResult ClearLogs(string logType, bool is30Days = false)
    {
        try
        {
            bool success = false;
            string message = "";

            switch (logType.ToLower())
            {
                case "error":
                    success = svc.ClearErrorLogs(is30Days);
                    message = success ? $"錯誤日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "錯誤日誌清理失敗";
                    break;
                case "access":
                    success = svc.ClearAccessLogs(is30Days);
                    message = success ? $"存取記錄清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "存取記錄清理失敗";
                    break;
                case "system":
                    success = svc.ClearAccessLogs(is30Days);
                    message = success ? $"系統日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "系統日誌清理失敗";
                    break;
                default:
                    return Json(new { success = false, message = "不支援的日誌類型" });
            }

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"清理失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 清理快取
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ClearCache(string cacheType, bool is30Days = false)
    {
        try
        {
            bool success = false;
            string message = "";

            switch (cacheType.ToLower())
            {
                case "memory":
                case "database":
                case "file":
                case "all":
                    success = await svc.ClearAllCacheAsync();
                    message = success ? "快取清理成功" : "快取清理失敗";
                    break;
                default:
                    return Json(new { success = false, message = "不支援的快取類型" });
            }

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"清理失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 刷新日誌統計
    /// </summary>
    public IActionResult RefreshLogStats()
    {
        try
        {
            var logStats = svc.FetchLogStatistics();
            return Json(new { 
                success = true, 
                errorLogs = logStats.errorLogs,
                accessLogs = logStats.accessLogs
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刷新失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 刷新快取統計
    /// </summary>
    public IActionResult RefreshCacheStats()
    {
        try
        {
            var cacheStats = svc.FetchCacheStatistics();
            return Json(new { 
                success = true, 
                memoryCacheSize = cacheStats.memoryCacheSize,
                cacheEntries = cacheStats.cacheEntries,
                lastCleared = cacheStats.lastCleared.ToString("yyyy-MM-dd HH:mm")
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刷新失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 切換維護模式
    /// </summary>
    [HttpPost]
    public IActionResult ToggleMaintenanceMode(bool enabled)
    {
        try
        {
            var success = svc.ToggleMaintenanceMode(enabled);
            var message = success 
                ? (enabled ? "維護模式已啟用" : "維護模式已停用")
                : "切換維護模式失敗";
            
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 系統健康檢查
    /// </summary>
    [HttpGet]
    public IActionResult SystemHealth()
    {
        try
        {
            var healthInfo = svc.GetSystemHealth();
            return Json(new { success = true, data = healthInfo });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"健康檢查失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 匯出日誌
    /// </summary>
    [HttpGet]
    public IActionResult ExportLogs(string logType = "all")
    {
        try
        {
            var logData = svc.ExportLogs(logType);
            var fileName = $"logs_{logType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            return Json(new { 
                success = true, 
                data = logData,
                fileName = fileName,
                message = "日誌匯出成功"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"匯出失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 優化資料庫
    /// </summary>
    [HttpPost]
    public IActionResult OptimizeDatabase()
    {
        try
        {
            var result = svc.OptimizeDatabase();
            return Json(new { 
                success = result.success, 
                message = result.message,
                details = result.details
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"優化失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 重啟系統
    /// </summary>
    [HttpPost]
    public IActionResult RestartSystem()
    {
        try
        {
            var result = svc.RestartSystem();
            return Json(new { 
                success = result.success, 
                message = result.message
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"重啟失敗：{ex.Message}" });
        }
    }
    #endregion
}
