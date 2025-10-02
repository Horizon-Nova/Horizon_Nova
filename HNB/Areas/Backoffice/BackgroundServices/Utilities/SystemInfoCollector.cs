using System.Diagnostics;
using System.Management;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 系統資訊收集工具
/// 提供各種系統資訊的收集方法
/// </summary>
public static class SystemInfoCollector
{
    /// <summary>
    /// 取得 CPU 使用率
    /// </summary>
    public static async Task<decimal> GetCpuUsageAsync()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "cpu get loadpercentage /value",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("LoadPercentage="))
                {
                    if (decimal.TryParse(line.Split('=')[1], out decimal usage))
                    {
                        return usage;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得 CPU 使用率時發生錯誤: {ex.Message}");
        }

        return 0;
    }

    /// <summary>
    /// 取得記憶體使用資訊
    /// </summary>
    public static async Task<(decimal UsagePercent, decimal AvailableGb, decimal UsedGb)> GetMemoryUsageAsync()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024; // 轉換為位元組
                var freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024; // 轉換為位元組
                var usedMemory = totalMemory - freeMemory;

                var totalGb = totalMemory / (1024 * 1024 * 1024);
                var freeGb = freeMemory / (1024 * 1024 * 1024);
                var usedGb = usedMemory / (1024 * 1024 * 1024);

                var usagePercent = totalMemory > 0 ? (decimal)(usedMemory * 100) / totalMemory : 0;

                return (usagePercent, (decimal)freeGb, (decimal)usedGb);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得記憶體使用資訊時發生錯誤: {ex.Message}");
        }

        return (0, 0, 0);
    }

    /// <summary>
    /// 取得磁碟使用資訊
    /// </summary>
    public static async Task<List<DiskUsageInfo>> GetDiskUsageAsync()
    {
        var diskList = new List<DiskUsageInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var driveType = Convert.ToInt32(obj["DriveType"]);
                if (driveType == 3) // 固定磁碟
                {
                    var totalSize = Convert.ToUInt64(obj["Size"] ?? 0);
                    var freeSpace = Convert.ToUInt64(obj["FreeSpace"] ?? 0);
                    var usedSpace = totalSize - freeSpace;

                    var diskInfo = new DiskUsageInfo
                    {
                        DriveLetter = obj["DeviceID"]?.ToString() ?? "",
                        TotalSizeGb = (long)(totalSize / (1024 * 1024 * 1024)),
                        FreeSpaceGb = (long)(freeSpace / (1024 * 1024 * 1024)),
                        UsedSpaceGb = (long)(usedSpace / (1024 * 1024 * 1024)),
                        UsagePercent = totalSize > 0 ? (decimal)(usedSpace * 100) / totalSize : 0
                    };

                    diskList.Add(diskInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得磁碟使用資訊時發生錯誤: {ex.Message}");
        }

        return diskList;
    }

    /// <summary>
    /// 取得網路介面資訊
    /// </summary>
    public static async Task<List<NetworkInterfaceInfo>> GetNetworkInterfacesAsync()
    {
        var networkList = new List<NetworkInterfaceInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var networkInfo = new NetworkInterfaceInfo
                {
                    Name = obj["Name"]?.ToString() ?? "未知",
                    Description = obj["Description"]?.ToString() ?? "未知",
                    Manufacturer = obj["Manufacturer"]?.ToString() ?? "未知",
                    NetConnectionStatus = obj["NetConnectionStatus"]?.ToString() ?? "未知"
                };

                networkList.Add(networkInfo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得網路介面資訊時發生錯誤: {ex.Message}");
        }

        return networkList;
    }

    /// <summary>
    /// 取得系統溫度資訊（需要額外的硬體監控軟體支援）
    /// </summary>
    public static async Task<SystemTemperatureInfo> GetSystemTemperatureAsync()
    {
        var tempInfo = new SystemTemperatureInfo();

        try
        {
            tempInfo.CpuTemperature = await GetCpuTemperatureFromWmiAsync();
            tempInfo.GpuTemperature = await GetGpuTemperatureFromWmiAsync();
            tempInfo.MotherboardTemperature = await GetMotherboardTemperatureFromWmiAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得系統溫度資訊時發生錯誤: {ex.Message}");
        }

        return tempInfo;
    }

    /// <summary>
    /// 取得電源資訊
    /// </summary>
    public static async Task<PowerInfo> GetPowerInfoAsync()
    {
        var powerInfo = new PowerInfo();

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                powerInfo.BatteryLevel = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0);
                powerInfo.BatteryStatus = obj["BatteryStatus"]?.ToString() ?? "未知";
                powerInfo.PowerSupplyStatus = obj["PowerManagementCapabilities"] != null ? "支援" : "不支援";
                break;
            }

            // 取得電源供應器資訊
            using var psuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");
            var psuCollection = psuSearcher.Get();

            foreach (ManagementObject obj in psuCollection)
            {
                powerInfo.PowerSupplyInfo = obj["Description"]?.ToString() ?? "未知";
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得電源資訊時發生錯誤: {ex.Message}");
        }

        return powerInfo;
    }

    /// <summary>
    /// 取得系統效能計數器
    /// </summary>
    public static async Task<PerformanceCounters> GetPerformanceCountersAsync()
    {
        var counters = new PerformanceCounters();

        try
        {
            // CPU 使用率
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // 第一次調用返回 0
            await Task.Delay(1000); // 等待 1 秒
            counters.CpuUsage = (decimal)cpuCounter.NextValue();

            // 記憶體使用率
            using var memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            var availableMemory = memoryCounter.NextValue();
            var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024); // 轉換為 MB
            counters.MemoryUsage = totalMemory > 0 ? (decimal)((totalMemory - availableMemory) * 100) / totalMemory : 0;

            // 磁碟使用率
            using var diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            diskCounter.NextValue();
            await Task.Delay(1000);
            counters.DiskUsage = (decimal)diskCounter.NextValue();

            // 網路使用率
            using var networkCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", GetActiveNetworkInterface());
            networkCounter.NextValue();
            await Task.Delay(1000);
            counters.NetworkUsage = (decimal)networkCounter.NextValue();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得效能計數器時發生錯誤: {ex.Message}");
        }

        return counters;
    }

    // 私有輔助方法
    private static async Task<int> GetCpuTemperatureFromWmiAsync()
    {
        // TODO: CPU 溫度檢測 - WMI MSAcpi_ThermalZoneTemperature 在大多數系統上不可用
        // 未來可考慮整合：
        // 1. OpenHardwareMonitor/LibreHardwareMonitor
        // 2. 特定 CPU 廠商的監控工具
        // 3. 第三方硬體監控庫
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM MSAcpi_ThermalZoneTemperature");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var temp = Convert.ToInt32(obj["CurrentTemperature"] ?? 0);
                if (temp > 0)
                {
                    return (temp - 2732) / 10; // 轉換為攝氏度
                }
            }
        }
        catch { }
        return 0; // 返回 0 表示無法檢測，後續會轉為 "null"
    }

    private static async Task<int> GetGpuTemperatureFromWmiAsync()
    {
        // TODO: GPU 溫度檢測 - .NET WMI 無法直接取得 GPU 溫度
        // 未來可考慮整合：
        // 1. OpenHardwareMonitor/LibreHardwareMonitor WMI 介面
        // 2. NVIDIA-SMI 命令列工具
        // 3. AMD GPU 專用 API
        // 4. 第三方硬體監控庫
        return await Task.FromResult(0);
    }

    private static async Task<int> GetMotherboardTemperatureFromWmiAsync()
    {
        // TODO: 主機板溫度檢測 - Win32_TemperatureProbe 在大多數系統上不提供資料
        // 未來可考慮整合：
        // 1. OpenHardwareMonitor/LibreHardwareMonitor
        // 2. 主機板廠商專用監控工具
        // 3. BIOS/UEFI 溫度感測器介面
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TemperatureProbe");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var temp = Convert.ToInt32(obj["CurrentReading"] ?? 0);
                if (temp > 0)
                {
                    return temp;
                }
            }
        }
        catch { }
        return 0; // 返回 0 表示無法檢測，後續會轉為 "null"
    }

    private static string GetActiveNetworkInterface()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var name = obj["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && !name.Contains("Loopback"))
                {
                    return name;
                }
            }
        }
        catch { }
        return "_Total";
    }

    // 資料模型類別
    public class DiskUsageInfo
    {
        public string DriveLetter { get; set; } = "";
        public long TotalSizeGb { get; set; }
        public long FreeSpaceGb { get; set; }
        public long UsedSpaceGb { get; set; }
        public decimal UsagePercent { get; set; }
    }

    public class NetworkInterfaceInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string NetConnectionStatus { get; set; } = "";
    }

    public class SystemTemperatureInfo
    {
        public int CpuTemperature { get; set; }
        public int GpuTemperature { get; set; }
        public int MotherboardTemperature { get; set; }
        public int MaxTemperature { get; set; } = 100;
    }

    public class PowerInfo
    {
        public int BatteryLevel { get; set; }
        public string BatteryStatus { get; set; } = "未知";
        public string PowerSupplyStatus { get; set; } = "未知";
        public string PowerSupplyInfo { get; set; } = "未知";
    }

    public class PerformanceCounters
    {
        public decimal CpuUsage { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal DiskUsage { get; set; }
        public decimal NetworkUsage { get; set; }
    }

    public class GpuInfo
    {
        public string Model { get; set; } = "未知";
        public string Manufacturer { get; set; } = "未知";
        public string MemorySize { get; set; } = "未知";
        public string DriverVersion { get; set; } = "未知";
        public int Temperature { get; set; } = 0;
        public decimal UsagePercent { get; set; } = 0;
    }

    /// <summary>
    /// 取得 GPU 資訊
    /// </summary>
    public static async Task<GpuInfo> GetGpuInfoAsync()
    {
        var gpuInfo = new GpuInfo();

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            using var collection = searcher.Get();

            foreach (ManagementObject queryObj in collection)
            {
                var name = queryObj["Name"]?.ToString();
                var manufacturer = queryObj["AdapterCompatibility"]?.ToString();
                var driverVersion = queryObj["DriverVersion"]?.ToString();
                var adapterRAM = queryObj["AdapterRAM"];

                // 只處理獨立顯卡，跳過內建顯卡和基本顯示適配器
                if (!string.IsNullOrEmpty(name) && 
                    !name.Contains("Microsoft") && 
                    !name.Contains("Basic") &&
                    !name.Contains("Generic") &&
                    adapterRAM != null)
                {
                    gpuInfo.Model = name;
                    gpuInfo.Manufacturer = manufacturer ?? "未知";
                    gpuInfo.DriverVersion = driverVersion ?? "未知";
                    
                    // 轉換記憶體大小
                    if (long.TryParse(adapterRAM.ToString(), out long ramBytes) && ramBytes > 0)
                    {
                        var ramGB = ramBytes / (1024.0 * 1024.0 * 1024.0);
                        gpuInfo.MemorySize = $"{ramGB:F1}GB";
                    }
                    
                    break; // 只取第一個獨立顯卡
                }
            }

            // 如果沒有找到獨立顯卡，設為 null 表示沒有檢測到
            if (gpuInfo.Model == "未知")
            {
                gpuInfo.Model = "null";
                gpuInfo.Manufacturer = "null";
                gpuInfo.MemorySize = "null";
                gpuInfo.DriverVersion = "null";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"取得 GPU 資訊時發生錯誤: {ex.Message}");
            gpuInfo.Model = "null";
            gpuInfo.Manufacturer = "null";
            gpuInfo.MemorySize = "null";
            gpuInfo.DriverVersion = "null";
        }

        return gpuInfo;
    }
}
