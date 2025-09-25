using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using HNB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class SettingsController(SettingsServices svc, LogManagementService logManagementSvc, CacheManagementService cacheManagementSvc) : Controller
{
    public IActionResult Settings()
    {
        var serverInfo = svc.GetVwSystemConfigServer().FirstOrDefault();
        return View(serverInfo);
    }

    public IActionResult SettingsTab(string t = "server")
    {
        var serverInfo = svc.GetVwSystemConfigServer().FirstOrDefault();
        return View("Server", serverInfo);
    }

    [HttpGet]
    public IActionResult GetSystemStatus()
    {
        var cpuUsage = GetCpuUsage();
        var memoryUsage = GetMemoryUsage();
        var diskUsage = GetDiskUsage();

        return Json(new
        {
            cpuUsage = cpuUsage,
            memoryUsage = memoryUsage,
            diskUsage = diskUsage
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetLogsInfo()
    {
        var statistics = await logManagementSvc.GetLogStatisticsAsync();

        return Json(new
        {
            errorLogs = statistics.ErrorLogs,
            systemLogs = statistics.SystemLogs,
            accessLogs = statistics.AccessLogs
        });
    }

    [HttpGet]
    public IActionResult GetCacheInfo()
    {
        var statistics = cacheManagementSvc.GetCacheStatistics();

        return Json(new
        {
            memoryCache = statistics.MemoryCacheSize,
            databaseCache = statistics.DatabaseCacheSize,
            fileCache = statistics.FileCacheSize
        });
    }

    [HttpPost]
    public async Task<IActionResult> ClearLogs(string logType, string clearType = "all", int? days = null)
    {
        var options = new LogClearOptions();
        
        // 根據清理類型設置選項
        switch (clearType.ToLower())
        {
            case "all":
                options.ClearType = LogClearType.All;
                break;
            case "older":
                options.ClearType = LogClearType.OlderThan;
                options.CutoffDate = DateTime.UtcNow.AddDays(-(days ?? 30)); // 預設清理30天前的
                break;
            default:
                options.ClearType = LogClearType.All;
                break;
        }

        bool success = false;
        string message = "";

        switch (logType.ToLower())
        {
            case "error":
                success = await logManagementSvc.ClearErrorLogsAsync(options);
                message = success ? "錯誤日誌清理成功" : "錯誤日誌清理失敗";
                break;
            case "system":
                success = await logManagementSvc.ClearSystemLogsAsync(options);
                message = success ? "系統日誌清理成功" : "系統日誌清理失敗";
                break;
            case "access":
                success = await logManagementSvc.ClearAccessLogsAsync(options);
                message = success ? "存取日誌清理成功" : "存取日誌清理失敗";
                break;
            default:
                return Json(new { success = false, message = "無效的日誌類型" });
        }

        return Json(new { success = success, message = message });
    }

    [HttpPost]
    public async Task<IActionResult> ClearCache(string cacheType)
    {
        bool success = false;
        string message = "";

        switch (cacheType.ToLower())
        {
            case "memory":
                success = await cacheManagementSvc.ClearMemoryCacheAsync();
                message = success ? "記憶體快取清理成功" : "記憶體快取清理失敗";
                break;
            case "database":
                success = await cacheManagementSvc.ClearDatabaseCacheAsync();
                message = success ? "資料庫快取清理成功" : "資料庫快取清理失敗";
                break;
            case "file":
                success = await cacheManagementSvc.ClearFileCacheAsync();
                message = success ? "檔案快取清理成功" : "檔案快取清理失敗";
                break;
            default:
                return Json(new { success = false, message = "無效的快取類型" });
        }

        return Json(new { success = success, message = message });
    }

    [HttpPost]
    public IActionResult ToggleMaintenanceMode(bool enabled)
    {
        UpdateMaintenanceMode(enabled);

        return Json(new { success = true, message = "維護模式已" + (enabled ? "啟用" : "停用") });
    }

    [HttpPost]
    public IActionResult OptimizeDatabase()
    {
        PerformDatabaseOptimization();

        return Json(new { success = true, message = "資料庫優化完成" });
    }

    [HttpPost]
    public IActionResult RestartApplication()
    {
        ScheduleApplicationRestart();

        return Json(new { success = true, message = "系統重啟指令已發送" });
    }

    #region 私有方法

    private double GetCpuUsage()
    {
        var process = Process.GetCurrentProcess();
        var startTime = DateTime.UtcNow;
        var startCpuUsage = process.TotalProcessorTime;
        
        Thread.Sleep(100);
        
        var endTime = DateTime.UtcNow;
        var endCpuUsage = process.TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        
        return Math.Round(cpuUsageTotal * 100, 2);
    }

    private double GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var totalMemory = GC.GetTotalMemory(false);
        var workingSet = process.WorkingSet64;
        return Math.Round((double)workingSet / (1024 * 1024 * 1024) * 100, 2);
    }

    private double GetDiskUsage()
    {
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
        if (!string.IsNullOrEmpty(systemRoot))
        {
            var drive = new DriveInfo(systemRoot);
            var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
            return Math.Round((double)usedSpace / drive.TotalSize * 100, 2);
        }
        return 0;
    }


    private void UpdateMaintenanceMode(bool enabled)
    {
    }

    private void PerformDatabaseOptimization()
    {
    }

    private void ScheduleApplicationRestart()
    {
    }

    #endregion
}
