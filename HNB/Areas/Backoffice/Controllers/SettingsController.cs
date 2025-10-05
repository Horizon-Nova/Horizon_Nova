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
        
        // 使用統一的 ViewBag 模型設置
        svc.ViewBagModel(ViewBag);
        
        return View();
    }

    #region AJAX 端點
    /// <summary>
    /// 清理日誌
    /// </summary>
    [HttpPost]
    public IActionResult ClearLogs(string logType, bool is30Days = false)
    {
        var (success, message) = logType.ToLower() switch
        {
            "error" => (svc.ClearErrorLogs(is30Days), svc.ClearErrorLogs(is30Days) ? $"錯誤日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "錯誤日誌清理失敗"),
            "access" => (svc.ClearAccessLogs(is30Days), svc.ClearAccessLogs(is30Days) ? $"存取記錄清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "存取記錄清理失敗"),
            "system" => (svc.ClearAccessLogs(is30Days), svc.ClearAccessLogs(is30Days) ? $"系統日誌清理成功{(is30Days ? "（30天前）" : "（全部）")}" : "系統日誌清理失敗"),
            _ => (false, "不支援的日誌類型")
        };

        return Json(new { success, message });
    }

    /// <summary>
    /// 清理快取
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ClearCache(string cacheType, bool is30Days = false)
    {
        var (success, message) = cacheType.ToLower() switch
        {
            "memory" or "database" or "file" or "all" => (await svc.ClearAllCacheAsync(), await svc.ClearAllCacheAsync() ? "快取清理成功" : "快取清理失敗"),
            _ => (false, "不支援的快取類型")
        };

        return Json(new { success, message });
    }

    /// <summary>
    /// 刷新日誌統計
    /// </summary>
    public IActionResult RefreshLogStats()
    {
        var logStats = svc.FetchLogStatistics();
        return Json(new { 
            success = true, 
            errorLogs = logStats.errorLogs,
            accessLogs = logStats.accessLogs
        });
    }

    /// <summary>
    /// 刷新快取統計
    /// </summary>
    public IActionResult RefreshCacheStats()
    {
        var cacheStats = svc.FetchCacheStatistics();
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
        var success = svc.ToggleMaintenanceMode(enabled);
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
        var healthInfo = svc.GetSystemHealth();
        return Json(new { success = true, data = healthInfo });
    }

    /// <summary>
    /// 匯出日誌
    /// </summary>
    [HttpGet]
    public IActionResult ExportLogs(string logType = "all")
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

    /// <summary>
    /// 優化資料庫
    /// </summary>
    [HttpPost]
    public IActionResult OptimizeDatabase()
    {
        var result = svc.OptimizeDatabase();
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
        var result = svc.RestartSystem();
        return Json(new { 
            success = result.success, 
            message = result.message
        });
    }
    #endregion
}
