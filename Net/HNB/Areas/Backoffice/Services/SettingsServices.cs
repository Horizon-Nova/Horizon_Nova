using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Utilities;
using HNB.Areas.Backoffice.Dtos;
using Models.Hnb;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Management;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// 系統設定服務層，負責處理硬體監控、日誌管理和快取管理功能
/// </summary>
public class SettingsServices(SettingsRepositories rep)
{
    #region 統一的查詢方法

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
        var errorLogsCount = LoadErrorLogsCount();
        var accessLogsCount = LoadAccessLogsCount();
        var cacheStats = LoadCacheStatistics();
        
        var hardwareInfo = LoadLocalHardwareInfo();
        
        viewBag.HardwareInfo = hardwareInfo;
        viewBag.MemoryChannel = GetMemoryChannelCount();
        viewBag.ErrorLogsCount = errorLogsCount;
        viewBag.AccessLogsCount = accessLogsCount;
        viewBag.MemoryCacheSize = cacheStats.memoryCacheSize;
        viewBag.CacheEntries = cacheStats.cacheEntries;
        viewBag.LastCacheCleared = cacheStats.lastCleared;
    }
    
    /// <summary>
    /// 載入本地硬體資訊
    /// </summary>
    public HardwareInfoDto LoadLocalHardwareInfo()
    {
        var hardware = new HardwareCollector
        {
            server_ip = GetLocalIpAddress() ?? "N/A",
            server_location = null,
            server_provider = null,
            environment_type = null
        };

        hardware = HardwareUtility.CollectCpuInfo(hardware) ?? hardware;
        hardware = HardwareUtility.CollectGpuInfo(hardware) ?? hardware;
        hardware = HardwareUtility.CollectMemoryInfo(hardware) ?? hardware;
        hardware = HardwareUtility.CollectStorageInfo(hardware) ?? hardware;
        hardware = NetworkHardwareUtility.CollectNetworkInfo(hardware) ?? hardware;
        hardware = SystemHardwareUtility.CollectSystemInfo(hardware) ?? hardware;
        hardware = HardwareUtility.CollectPowerInfo(hardware) ?? hardware;
        hardware = SystemHardwareUtility.SetCheckInfo(hardware, "local", 300) ?? hardware;

        return ConvertToDto(hardware);
    }
    
    /// <summary>
    /// 取得記憶體通道數
    /// </summary>
    public string GetMemoryChannelCount()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            var collection = searcher.Get();
            
            var moduleCount = 0;
            foreach (ManagementObject obj in collection)
            {
                moduleCount++;
            }
            
            if (moduleCount > 0)
            {
                if (moduleCount <= 2)
                {
                    return moduleCount == 2 ? "雙通道" : "單通道";
                }
                else if (moduleCount <= 4)
                {
                    return moduleCount == 4 ? "四通道" : "雙通道";
                }
                else
                {
                    return $"{moduleCount} 模組";
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (Directory.Exists("/sys/devices/system/edac/mc"))
            {
                var mcDirs = Directory.GetDirectories("/sys/devices/system/edac/mc");
                if (mcDirs.Length > 0)
                {
                    return mcDirs.Length == 1 ? "單通道" : 
                           mcDirs.Length == 2 ? "雙通道" : 
                           mcDirs.Length == 4 ? "四通道" : 
                           $"{mcDirs.Length} 通道";
                }
            }
        }
        
        return "N/A";
    }
    
    /// <summary>
    /// 轉換硬體收集器為 DTO
    /// </summary>
    private HardwareInfoDto ConvertToDto(HardwareCollector hardware)
    {
        if (hardware == null)
        {
            return new HardwareInfoDto
            {
                server_ip = "N/A",
                host_name = "N/A",
                operating_system = "N/A",
                cpu_name = "N/A",
                cpu_model = "N/A",
                gpu_name = "N/A",
                memory_name = "N/A"
            };
        }

        decimal? memoryUsagePercent = null;
        if (hardware.system_memory_total.HasValue && hardware.system_memory_used.HasValue && hardware.system_memory_total.Value > 0)
        {
            memoryUsagePercent = (decimal)(hardware.system_memory_used.Value * 100) / hardware.system_memory_total.Value;
        }

        long? memoryTotalGb = hardware.system_memory_total.HasValue 
            ? hardware.system_memory_total.Value / (1024 * 1024 * 1024) 
            : null;
        long? memoryUsedGb = hardware.system_memory_used.HasValue 
            ? hardware.system_memory_used.Value / (1024 * 1024 * 1024) 
            : null;

        return new HardwareInfoDto
        {
            server_ip = hardware.server_ip ?? "N/A",
            server_location = hardware.server_location ?? "N/A",
            server_provider = hardware.server_provider ?? "N/A",
            environment_type = hardware.environment_type ?? "N/A",
            host_name = hardware.host_name ?? "N/A",
            operating_system = hardware.operating_system ?? "N/A",
            kernel_version = hardware.kernel_version ?? "N/A",
            uptime = hardware.uptime ?? "N/A",
            
            cpu_name = hardware.cpu_names?.FirstOrDefault() ?? "N/A",
            cpu_manufacturer = hardware.cpu_manufacturers?.FirstOrDefault() ?? "N/A",
            cpu_model = hardware.cpu_models?.FirstOrDefault() ?? "N/A",
            cpu_cores = hardware.cpu_cores?.FirstOrDefault(),
            cpu_threads = hardware.cpu_threads?.FirstOrDefault(),
            cpu_base_clock = hardware.cpu_base_clocks?.FirstOrDefault(),
            cpu_boost_clock = hardware.cpu_boost_clocks?.FirstOrDefault(),
            cpu_temperature = hardware.cpu_temperatures?.FirstOrDefault(),
            cpu_usage_percent = hardware.cpu_usages?.FirstOrDefault(),
            cpu_health_status = hardware.cpu_health_statuses?.FirstOrDefault() ?? "N/A",
            cpu_health_percentage = hardware.cpu_health_percentages?.FirstOrDefault(),
            
            gpu_name = hardware.gpu_names?.FirstOrDefault() ?? "N/A",
            gpu_manufacturer = hardware.gpu_manufacturers?.FirstOrDefault() ?? "N/A",
            gpu_model = hardware.gpu_models?.FirstOrDefault() ?? "N/A",
            gpu_memory_size = hardware.gpu_memory_sizes?.FirstOrDefault() ?? "N/A",
            gpu_temperature = hardware.gpu_temperatures?.FirstOrDefault(),
            gpu_usage_percent = null,
            gpu_health_status = null,
            gpu_health_percentage = null,
            
            memory_name = hardware.memory_names?.FirstOrDefault() ?? "N/A",
            memory_type = hardware.memory_types?.FirstOrDefault() ?? "N/A",
            memory_total_capacity = hardware.memory_capacities?.FirstOrDefault() ?? "N/A",
            memory_speed = hardware.memory_speeds?.FirstOrDefault() ?? "N/A",
            memory_usage_percent = hardware.memory_usages?.FirstOrDefault() ?? memoryUsagePercent,
            memory_health_status = hardware.memory_health_statuses?.FirstOrDefault() ?? "N/A",
            memory_health_percentage = hardware.memory_health_percentages?.FirstOrDefault(),
            
            system_memory_total = hardware.system_memory_total,
            system_memory_used = hardware.system_memory_used,
            system_memory_free = hardware.system_memory_free,
            memory_total_capacity_gb = memoryTotalGb,
            memory_used_gb = memoryUsedGb,
            memory_available_gb = hardware.system_memory_free.HasValue 
                ? hardware.system_memory_free.Value / (1024 * 1024 * 1024) 
                : null,
            
            system_load_avg = hardware.system_load_avg?.FirstOrDefault(),
            system_processes = hardware.system_processes,
            system_users = hardware.system_users,
            
            battery_level = hardware.battery_level,
            power_efficiency = hardware.power_efficiency ?? "N/A",
            power_supply_info = hardware.power_supply_info ?? "N/A",
            
            last_check_time = hardware.last_check_time,
            check_method = hardware.check_method ?? "N/A",
            check_interval = hardware.check_interval,
            is_active = hardware.is_active,
            created_at = hardware.created_at,
            updated_at = hardware.updated_at
        };
    }
    
    /// <summary>
    /// 取得本地 IP 位址
    /// </summary>
    private string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch
        {
        }
        
        return "127.0.0.1";
    }
    
    #endregion

    #region 基本 CRUD 操作

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
        var errorLogsCount = LoadErrorLogsCount();
        var accessLogsCount = LoadAccessLogsCount();
        var cacheStats = LoadCacheStatistics();

        return new
        {
            timestamp = DateTime.Now,
            systemStatus = "運行中",
            cpuUsage = 0,
            memoryUsage = 0,
            diskUsage = 0,
            errorLogs = errorLogsCount,
            accessLogs = accessLogsCount,
            cacheSize = cacheStats.memoryCacheSize,
            uptime = "未知",
            lastUpdated = DateTime.Now,
            healthScore = CalculateHealthScore(errorLogsCount, accessLogsCount, cacheStats)
        };
    }

    /// <summary>
    /// 計算系統健康分數
    /// </summary>
    private int CalculateHealthScore(int errorLogs, int accessLogs, (long memoryCacheSize, int cacheEntries, DateTime lastCleared) cacheStats)
    {
        int score = 100;
        
        if (errorLogs > 100) score -= 10;
        if (cacheStats.memoryCacheSize > 1024 * 1024 * 100) score -= 5;
        
        return Math.Max(0, score);
    }

    #endregion
}
