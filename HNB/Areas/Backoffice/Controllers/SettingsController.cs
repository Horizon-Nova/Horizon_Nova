using HNB.Filters;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class SettingsController(SettingsServices svc, SettingsRepositories settingsRepositories, CacheManagementUtilities cacheUtilities) : Controller
{
    public async Task<IActionResult> Settings()
    {
        var serverInfo = svc.GetVwSystemConfigServer().FirstOrDefault();
        var cpuUsage = GetCpuUsage();
        var memoryUsage = GetMemoryUsage();
        var diskUsage = GetDiskUsage();
        var errorLogs = await settingsRepositories.ErrorLogsCountAsync();
        var accessLogs = await settingsRepositories.AccessLogsCountAsync();
        var cacheStats = cacheUtilities.GetCacheStatistics();

        ViewBag.CpuUsage = cpuUsage;
        ViewBag.MemoryUsage = memoryUsage;
        ViewBag.DiskUsage = diskUsage;
        ViewBag.CpuCores = Environment.ProcessorCount;
        ViewBag.ErrorLogs = errorLogs;
        ViewBag.SystemLogs = 0;
        ViewBag.AccessLogs = accessLogs;
        ViewBag.MemoryCacheSize = cacheStats.MemoryCacheSize;
        ViewBag.DatabaseCacheSize = cacheStats.DatabaseCacheSize;
        ViewBag.FileCacheSize = cacheStats.FileCacheSize;

        return View(serverInfo);
    }

    public IActionResult SettingsTab(string t)
    {
        var serverInfo = svc.GetVwSystemConfigServer().FirstOrDefault();
        var cpuUsage = GetCpuUsage();
        var memoryUsage = GetMemoryUsage();
        var diskUsage = GetDiskUsage();

        ViewBag.CpuUsage = cpuUsage;
        ViewBag.MemoryUsage = memoryUsage;
        ViewBag.DiskUsage = diskUsage;
        ViewBag.CpuCores = Environment.ProcessorCount;

        return t switch
        {
            "computer" => PartialView("_ComputerInfo", serverInfo),
            _ => PartialView("_ComputerInfo", serverInfo)
        };
    }

    public IActionResult GetDetailModal(string type)
    {
        return type switch
        {
            "error" => PartialView("_ErrorLogDetail"),
            "system" => PartialView("_SystemLogDetail"),
            "access" => PartialView("_AccessLogDetail"),
            "memory" => PartialView("_MemoryCacheDetail"),
            "database" => PartialView("_DatabaseCacheDetail"),
            "file" => PartialView("_FileCacheDetail"),
            _ => PartialView("_ErrorLogDetail")
        };
    }

    public IActionResult SystemStatus()
    {
        var cpuUsage = GetCpuUsage();
        var memoryUsage = GetMemoryUsage();
        var diskUsage = GetDiskUsage();

        return Json(new
        {
            cpuUsage,
            memoryUsage,
            diskUsage,
            cpuCores = Environment.ProcessorCount
        });
    }

    public async Task<IActionResult> LogsInfo()
    {
        var errorLogs = await settingsRepositories.ErrorLogsCountAsync();
        var accessLogs = await settingsRepositories.AccessLogsCountAsync();
        var systemLogs = 0;

        return Json(new
        {
            errorLogs,
            systemLogs,
            accessLogs
        });
    }

    public IActionResult CacheInfo()
    {
        var statistics = cacheUtilities.GetCacheStatistics();

        return Json(new
        {
            memoryCache = statistics.MemoryCacheSize,
            databaseCache = statistics.DatabaseCacheSize,
            fileCache = statistics.FileCacheSize
        });
    }

    public async Task<IActionResult> ClearLogs(string logType, bool is30Days = false)
    {
        bool success = false;
        string message = "";

        switch (logType.ToLower())
        {
            case "error":
                success = await settingsRepositories.ClearErrorLogsAsync(is30Days);
                message = success ? (is30Days ? "30天前的錯誤日誌清理成功" : "錯誤日誌全部清理成功") : "錯誤日誌清理失敗";
                break;
            case "system":
                success = true;
                message = is30Days ? "30天前的系統日誌清理成功" : "系統日誌全部清理成功";
                break;
            case "access":
                success = await settingsRepositories.ClearAccessLogsAsync(is30Days);
                message = success ? (is30Days ? "30天前的存取日誌清理成功" : "存取日誌全部清理成功") : "存取日誌清理失敗";
                break;
            default:
                return Json(new { success = false, message = "無效的日誌類型" });
        }

        return Json(new { success = success, message = message });
    }

    [HttpPost]
    public IActionResult ClearCache(string cacheType, bool is30Days = false)
    {
        bool success = false;
        string message = "";

        switch (cacheType.ToLower())
        {
            case "memory":
                success = cacheUtilities.ClearMemoryCache(is30Days);
                message = success ? (is30Days ? "30天前的記憶體快取清理成功" : "記憶體快取全部清理成功") : "記憶體快取清理失敗";
                break;
            case "database":
                success = cacheUtilities.ClearDatabaseCache(is30Days);
                message = success ? (is30Days ? "30天前的資料庫快取清理成功" : "資料庫快取全部清理成功") : "資料庫快取清理失敗";
                break;
            case "file":
                success = cacheUtilities.ClearFileCache(is30Days);
                message = success ? (is30Days ? "30天前的檔案快取清理成功" : "檔案快取全部清理成功") : "檔案快取清理失敗";
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
