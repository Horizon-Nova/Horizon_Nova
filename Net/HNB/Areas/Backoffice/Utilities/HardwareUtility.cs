using HNB.Areas.Backoffice.Dtos;
using System.Runtime.InteropServices;
using System.Management;

namespace HNB.Areas.Backoffice.Utilities;

/// <summary>
/// 硬體工具類別
/// 負責收集和處理硬體相關的資訊（CPU、GPU、記憶體、儲存、電源）
/// </summary>
public static class HardwareUtility
{
    #region CPU 資訊收集

    /// <summary>
    /// 收集 CPU 資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectCpuInfo(HardwareCollector hardware)
    {
        var processorId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "N/A";
        var processorCount = Environment.ProcessorCount;

        hardware.cpu_names = new List<string> { processorId };
        hardware.cpu_manufacturers = new List<string> { IdentifyCpuManufacturer(processorId) };
        hardware.cpu_models = new List<string> { processorId };
        hardware.cpu_cores = new List<int> { processorCount };
        hardware.cpu_threads = new List<int> { processorCount };
        
        var clockInfo = CollectCpuClockSpeed();
        hardware.cpu_base_clocks = clockInfo.baseClocks;
        hardware.cpu_boost_clocks = clockInfo.boostClocks;
        
        var cpuUsage = CollectCpuUsage();
        hardware.cpu_usages = cpuUsage;
        
        hardware.cpu_temperatures = null;
        
        hardware.cpu_health_statuses = CalculateCpuHealthStatus(cpuUsage);
        hardware.cpu_health_percentages = CalculateCpuHealthPercentage(cpuUsage);

        return hardware;
    }
    
    /// <summary>
    /// 收集 CPU 時脈速度
    /// </summary>
    private static (List<decimal>? baseClocks, List<decimal>? boostClocks) CollectCpuClockSpeed()
    {
        var baseClocks = new List<decimal>();
        var boostClocks = new List<decimal>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed, CurrentClockSpeed FROM Win32_Processor");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var maxClock = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0);
                var currentClock = Convert.ToUInt32(obj["CurrentClockSpeed"] ?? 0);
                
                if (maxClock > 0)
                {
                    var baseClock = (decimal)maxClock / 1000;
                    var boostClock = currentClock > maxClock ? (decimal)currentClock / 1000 : baseClock;
                    
                    baseClocks.Add(baseClock);
                    boostClocks.Add(boostClock);
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/cpuinfo"))
            {
                var cpuInfo = File.ReadAllText("/proc/cpuinfo");
                var lines = cpuInfo.Split('\n');
                
                decimal? baseClock = null;
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("cpu MHz"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length >= 2 && decimal.TryParse(parts[1].Trim(), out var clockMHz))
                        {
                            var clockGHz = clockMHz / 1000;
                            if (!baseClock.HasValue || clockGHz > baseClock.Value)
                            {
                                baseClock = clockGHz;
                            }
                        }
                    }
                }
                
                if (baseClock.HasValue)
                {
                    baseClocks.Add(baseClock.Value);
                    boostClocks.Add(baseClock.Value);
                }
            }
        }

        return (baseClocks.Count > 0 ? baseClocks : null, boostClocks.Count > 0 ? boostClocks : null);
    }
    
    /// <summary>
    /// 收集 CPU 使用率
    /// </summary>
    private static List<decimal>? CollectCpuUsage()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
                var collection = searcher.Get();
                var usages = new List<decimal>();

                foreach (ManagementObject obj in collection)
                {
                    var load = Convert.ToUInt32(obj["LoadPercentage"] ?? 0);
                    usages.Add(load);
                }

                return usages.Count > 0 ? usages : null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/proc/stat"))
                {
                    var statContent = File.ReadAllText("/proc/stat");
                    var lines = statContent.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("cpu "))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 8)
                            {
                                var total = long.Parse(parts[1]) + long.Parse(parts[2]) + long.Parse(parts[3]) + 
                                           long.Parse(parts[4]) + long.Parse(parts[5]) + long.Parse(parts[6]) + long.Parse(parts[7]);
                                var idle = long.Parse(parts[4]);
                                var usage = total > 0 ? (decimal)((total - idle) * 100) / total : 0;
                                
                                return new List<decimal> { usage };
                            }
                        }
                    }
                }
            }
        }
        catch
        {
        }

        return null;
    }
    
    /// <summary>
    /// 計算 CPU 健康狀態
    /// </summary>
    private static List<string>? CalculateCpuHealthStatus(List<decimal>? usages)
    {
        if (usages == null || usages.Count == 0)
            return null;

        var statuses = new List<string>();
        foreach (var usage in usages)
        {
            if (usage > 90)
                statuses.Add("警告");
            else if (usage > 70)
                statuses.Add("正常");
            else
                statuses.Add("良好");
        }

        return statuses;
    }
    
    /// <summary>
    /// 計算 CPU 健康百分比（基於使用率，使用率越低健康度越高）
    /// </summary>
    private static List<int>? CalculateCpuHealthPercentage(List<decimal>? usages)
    {
        if (usages == null || usages.Count == 0)
            return null;

        var percentages = new List<int>();
        foreach (var usage in usages)
        {
            var health = (int)(100 - usage);
            percentages.Add(Math.Max(0, health));
        }

        return percentages;
    }

    /// <summary>
    /// 識別 CPU 製造商
    /// </summary>
    /// <param name="processorId">處理器識別碼</param>
    /// <returns>製造商名稱</returns>
    private static string IdentifyCpuManufacturer(string processorId) =>
        processorId.Contains("Intel", StringComparison.OrdinalIgnoreCase) ? "Intel" :
        processorId.Contains("AMD", StringComparison.OrdinalIgnoreCase) ? "AMD" :
        processorId.Contains("ARM", StringComparison.OrdinalIgnoreCase) ? "ARM" : "N/A";

    /// <summary>
    /// 驗證 CPU 資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateCpuInfo(HardwareCollector hardware) =>
        hardware.cpu_names?.Count > 0 && hardware.cpu_cores?.Count > 0;

    #endregion

    #region GPU 資訊收集

    /// <summary>
    /// 收集 GPU 資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectGpuInfo(HardwareCollector hardware)
    {
        var gpuNames = new List<string>();
        var gpuManufacturers = new List<string>();
        var gpuModels = new List<string>();
        var gpuMemorySizes = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            using var collection = searcher.Get();

            foreach (ManagementObject queryObj in collection)
            {
                var name = queryObj["Name"]?.ToString() ?? "N/A";
                var manufacturer = queryObj["AdapterCompatibility"]?.ToString() ?? "N/A";
                var adapterRAM = queryObj["AdapterRAM"];

                if (!name.Contains("Microsoft") && !name.Contains("Basic") && !name.Contains("Generic") && adapterRAM != null)
                {
                    gpuNames.Add(name);
                    gpuManufacturers.Add(manufacturer);
                    gpuModels.Add(name);
                    
                    var ramGB = long.TryParse(adapterRAM.ToString(), out long ramBytes) && ramBytes > 0 
                        ? $"{ramBytes / (1024.0 * 1024.0 * 1024.0):F1}GB" : "N/A";
                    gpuMemorySizes.Add(ramGB);
                }
            }
        }

        if (gpuNames.Count == 0)
        {
            gpuNames.Add("未檢測到獨立顯卡");
            gpuManufacturers.Add("N/A");
            gpuModels.Add("N/A");
            gpuMemorySizes.Add("N/A");
        }

        hardware.gpu_names = gpuNames;
        hardware.gpu_manufacturers = gpuManufacturers;
        hardware.gpu_models = gpuModels;
        hardware.gpu_memory_sizes = gpuMemorySizes;
        
        hardware.gpu_temperatures = null;

        return hardware;
    }

    /// <summary>
    /// 驗證 GPU 資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateGpuInfo(HardwareCollector hardware) =>
        hardware.gpu_names?.Count > 0;

    #endregion

    #region 記憶體資訊收集

    /// <summary>
    /// 收集記憶體資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectMemoryInfo(HardwareCollector hardware)
    {
        long totalMemory = 0;
        long freeMemory = 0;
        long usedMemory = 0;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                totalMemory = (long)(Convert.ToUInt64(obj["TotalVisibleMemorySize"] ?? 0) * 1024);
                freeMemory = (long)(Convert.ToUInt64(obj["FreePhysicalMemory"] ?? 0) * 1024);
                usedMemory = totalMemory - freeMemory;
                break;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/meminfo"))
            {
                var memInfo = File.ReadAllText("/proc/meminfo");
                var lines = memInfo.Split('\n');
                
                long memTotal = 0;
                long memAvailable = 0;
                long memFree = 0;
                long buffers = 0;
                long cached = 0;

                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                        memTotal = ParseMemInfoValue(line);
                    else if (line.StartsWith("MemAvailable:"))
                        memAvailable = ParseMemInfoValue(line);
                    else if (line.StartsWith("MemFree:"))
                        memFree = ParseMemInfoValue(line);
                    else if (line.StartsWith("Buffers:"))
                        buffers = ParseMemInfoValue(line);
                    else if (line.StartsWith("Cached:"))
                        cached = ParseMemInfoValue(line);
                }

                totalMemory = memTotal * 1024;
                freeMemory = memAvailable > 0 ? memAvailable * 1024 : (memFree + buffers + cached) * 1024;
                usedMemory = totalMemory - freeMemory;
            }
        }

        if (totalMemory > 0)
        {
            var totalGb = totalMemory / (1024 * 1024 * 1024);
            var usagePercent = totalMemory > 0 ? (decimal)(usedMemory * 100) / totalMemory : 0;

            hardware.system_memory_total = totalMemory;
            hardware.system_memory_used = usedMemory;
            hardware.system_memory_free = freeMemory;

            hardware.memory_names = new List<string> { "系統記憶體" };
            hardware.memory_types = null;
            hardware.memory_capacities = new List<string> { $"{totalGb}GB" };
            hardware.memory_speeds = null;
            hardware.memory_usages = new List<decimal> { usagePercent };
            
            hardware.memory_health_statuses = CalculateMemoryHealthStatus(usagePercent);
            hardware.memory_health_percentages = CalculateMemoryHealthPercentage(usagePercent);
        }
        else
        {
            hardware.system_memory_total = null;
            hardware.system_memory_used = null;
            hardware.system_memory_free = null;
            hardware.memory_names = null;
            hardware.memory_types = null;
            hardware.memory_capacities = null;
            hardware.memory_speeds = null;
            hardware.memory_usages = null;
            hardware.memory_health_statuses = null;
            hardware.memory_health_percentages = null;
        }

        return hardware;
    }
    
    /// <summary>
    /// 計算記憶體健康狀態
    /// </summary>
    private static List<string>? CalculateMemoryHealthStatus(decimal usagePercent)
    {
        if (usagePercent > 90)
            return new List<string> { "警告" };
        else if (usagePercent > 70)
            return new List<string> { "正常" };
        else
            return new List<string> { "良好" };
    }
    
    /// <summary>
    /// 計算記憶體健康百分比（使用率越低健康度越高）
    /// </summary>
    private static List<int>? CalculateMemoryHealthPercentage(decimal usagePercent)
    {
        var health = (int)(100 - usagePercent);
        return new List<int> { Math.Max(0, health) };
    }

    /// <summary>
    /// 解析 /proc/meminfo 的數值（Linux）
    /// </summary>
    private static long ParseMemInfoValue(string line)
    {
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && long.TryParse(parts[1], out var value))
        {
            return value;
        }
        return 0;
    }

    /// <summary>
    /// 驗證記憶體資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateMemoryInfo(HardwareCollector hardware) =>
        hardware.system_memory_total.HasValue && hardware.system_memory_total.Value > 0;

    #endregion

    #region 儲存裝置資訊收集

    /// <summary>
    /// 收集儲存資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectStorageInfo(HardwareCollector hardware)
    {
        var storageNames = new List<string>();
        var storageTypes = new List<string>();
        var storageCapacities = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var driveType = Convert.ToInt32(obj["DriveType"] ?? 0);
                if (driveType == 3)
                {
                    var driveLetter = obj["DeviceID"]?.ToString() ?? "N/A";
                    var totalSize = Convert.ToUInt64(obj["Size"] ?? 0);
                    var totalSizeGb = totalSize / (1024 * 1024 * 1024);

                    storageNames.Add($"磁碟 {driveLetter}");
                    storageCapacities.Add($"{totalSizeGb}GB");
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            foreach (var drive in drives)
            {
                var totalSize = drive.TotalSize;
                var totalSizeGb = totalSize / (1024 * 1024 * 1024);
                var mountPoint = drive.RootDirectory.FullName.TrimEnd('/', '\\');

                storageNames.Add(mountPoint);
                storageCapacities.Add($"{totalSizeGb}GB");
            }
        }

        if (storageNames.Count == 0)
        {
            storageNames.Add("未檢測到儲存裝置");
            storageCapacities.Add("N/A");
        }

        hardware.storage_names = storageNames;
        hardware.storage_types = null;
        hardware.storage_capacities = storageCapacities;
        hardware.storage_interfaces = null;
        hardware.storage_read_speeds = null;
        hardware.storage_write_speeds = null;

        return hardware;
    }

    /// <summary>
    /// 驗證儲存資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateStorageInfo(HardwareCollector hardware) =>
        hardware.storage_names?.Count > 0;

    #endregion

    #region 電源資訊收集

    /// <summary>
    /// 收集電源資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectPowerInfo(HardwareCollector hardware)
    {
        hardware.battery_level = null;
        hardware.power_efficiency = "N/A";
        hardware.power_supply_info = "N/A";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var batteryLevel = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0);
                var batteryStatus = obj["BatteryStatus"]?.ToString();

                if (batteryLevel > 0)
                {
                    hardware.battery_level = batteryLevel;
                    hardware.power_efficiency = batteryStatus ?? "N/A";
                }

                break;
            }

            if (hardware.battery_level == null)
            {
                hardware.power_efficiency = "AC 電源";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (Directory.Exists("/sys/class/power_supply"))
            {
                var powerSupplies = Directory.GetDirectories("/sys/class/power_supply");
                foreach (var ps in powerSupplies)
                {
                    var typeFile = Path.Combine(ps, "type");
                    if (File.Exists(typeFile))
                    {
                        var type = File.ReadAllText(typeFile).Trim();
                        if (type == "Battery")
                        {
                            var capacityFile = Path.Combine(ps, "capacity");
                            if (File.Exists(capacityFile) && int.TryParse(File.ReadAllText(capacityFile).Trim(), out var capacity))
                            {
                                hardware.battery_level = capacity;
                            }

                            var statusFile = Path.Combine(ps, "status");
                            if (File.Exists(statusFile))
                            {
                                hardware.power_efficiency = File.ReadAllText(statusFile).Trim();
                            }
                        }
                        else if (type == "Mains")
                        {
                            hardware.power_efficiency = "AC 電源";
                        }
                    }
                }
            }
        }

        return hardware;
    }

    /// <summary>
    /// 驗證電源資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidatePowerInfo(HardwareCollector hardware) =>
        hardware.battery_level.HasValue || 
        !string.IsNullOrEmpty(hardware.power_efficiency);

    #endregion
}

