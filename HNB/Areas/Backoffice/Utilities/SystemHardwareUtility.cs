using HNB.Areas.Backoffice.Dtos;
using System.Runtime.InteropServices;
using System.Linq;

namespace HNB.Areas.Backoffice.Utilities;

/// <summary>
/// 系統硬體工具類別
/// 負責收集和處理系統相關的資訊（系統資訊、檢查資訊）
/// </summary>
public static class SystemHardwareUtility
{
    #region 系統資訊收集

    /// <summary>
    /// 收集系統資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectSystemInfo(HardwareCollector hardware)
    {
        hardware.host_name = Environment.MachineName ?? "N/A";
        hardware.operating_system = Environment.OSVersion.VersionString ?? "N/A";
        hardware.kernel_version = GetKernelVersion();
        hardware.uptime = CalculateSystemUptime();

        hardware.system_load_avg = GetSystemLoadAvg();

        hardware.system_processes = System.Diagnostics.Process.GetProcesses().Length;
        hardware.system_users = null;

        var drives = DriveInfo.GetDrives();
        long totalDiskSpace = 0;
        long usedDiskSpace = 0;
        long freeDiskSpace = 0;

        foreach (var drive in drives.Where(d => d.IsReady))
        {
            totalDiskSpace += drive.TotalSize;
            usedDiskSpace += drive.TotalSize - drive.AvailableFreeSpace;
            freeDiskSpace += drive.AvailableFreeSpace;
        }

        hardware.system_disk_total = totalDiskSpace > 0 ? totalDiskSpace : null;
        hardware.system_disk_used = usedDiskSpace > 0 ? usedDiskSpace : null;
        hardware.system_disk_free = freeDiskSpace > 0 ? freeDiskSpace : null;

        hardware.system_swap_total = GetSwapTotal();
        hardware.system_swap_used = GetSwapUsed();

        return hardware;
    }

    /// <summary>
    /// 計算系統運行時間
    /// </summary>
    /// <returns>運行時間字串</returns>
    private static string CalculateSystemUptime()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}天 {uptime.Hours}小時 {uptime.Minutes}分鐘";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/uptime"))
            {
                var uptimeContent = File.ReadAllText("/proc/uptime").Trim();
                var parts = uptimeContent.Split(' ');
                if (parts.Length > 0 && double.TryParse(parts[0], out var uptimeSeconds))
                {
                    var uptime = TimeSpan.FromSeconds(uptimeSeconds);
                    return $"{uptime.Days}天 {uptime.Hours}小時 {uptime.Minutes}分鐘";
                }
            }
        }

        return "N/A";
    }

    /// <summary>
    /// 取得核心版本
    /// </summary>
    /// <returns>核心版本字串</returns>
    private static string GetKernelVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/version"))
            {
                var version = File.ReadAllText("/proc/version").Trim();
                var parts = version.Split(' ');
                if (parts.Length >= 3)
                {
                    return parts[2];
                }
            }
        }

        return Environment.OSVersion.Version.ToString();
    }

    /// <summary>
    /// 取得系統負載平均值（Linux）
    /// </summary>
    /// <returns>負載平均值列表</returns>
    private static List<decimal>? GetSystemLoadAvg()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/loadavg"))
            {
                var loadAvg = File.ReadAllText("/proc/loadavg").Trim();
                var parts = loadAvg.Split(' ');
                if (parts.Length >= 3)
                {
                    var loads = new List<decimal>();
                    foreach (var part in parts.Take(3))
                    {
                        if (decimal.TryParse(part, out var load))
                        {
                            loads.Add(load);
                        }
                    }
                    return loads.Count > 0 ? loads : null;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 取得交換空間總量（Linux）
    /// </summary>
    /// <returns>交換空間總量（bytes）</returns>
    private static long? GetSwapTotal()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/meminfo"))
            {
                var memInfo = File.ReadAllText("/proc/meminfo");
                var lines = memInfo.Split('\n');

                foreach (var line in lines)
                {
                    if (line.StartsWith("SwapTotal:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var swapKb))
                        {
                            return swapKb * 1024;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 取得已用交換空間（Linux）
    /// </summary>
    /// <returns>已用交換空間（bytes）</returns>
    private static long? GetSwapUsed()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists("/proc/meminfo"))
            {
                var memInfo = File.ReadAllText("/proc/meminfo");
                var lines = memInfo.Split('\n');

                long? swapTotal = null;
                long? swapFree = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("SwapTotal:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var swapKb))
                        {
                            swapTotal = swapKb * 1024;
                        }
                    }
                    else if (line.StartsWith("SwapFree:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var swapKb))
                        {
                            swapFree = swapKb * 1024;
                        }
                    }
                }

                if (swapTotal.HasValue && swapFree.HasValue)
                {
                    return swapTotal.Value - swapFree.Value;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 驗證系統資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateSystemInfo(HardwareCollector hardware) =>
        !string.IsNullOrEmpty(hardware.host_name) && 
        !string.IsNullOrEmpty(hardware.operating_system) &&
        hardware.system_processes.HasValue;

    #endregion

    #region 檢查資訊設定

    /// <summary>
    /// 設定檢查資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <param name="checkMethod">檢查方式</param>
    /// <param name="checkInterval">檢查間隔（秒）</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector SetCheckInfo(HardwareCollector hardware, string checkMethod = "local", int checkInterval = 300)
    {
        hardware.last_check_time = DateTime.UtcNow;
        hardware.check_method = checkMethod ?? "local";
        hardware.check_interval = checkInterval;
        hardware.is_active = true;

        hardware.created_at ??= DateTime.UtcNow;
        hardware.updated_at = DateTime.UtcNow;
        
        // 確保字串欄位不為 null
        hardware.server_ip ??= "N/A";
        hardware.server_location ??= "N/A";
        hardware.server_provider ??= "N/A";
        hardware.environment_type ??= "N/A";
        hardware.host_name ??= "N/A";
        hardware.operating_system ??= "N/A";
        hardware.kernel_version ??= "N/A";
        hardware.uptime ??= "N/A";

        return hardware;
    }

    /// <summary>
    /// 驗證檢查資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateCheckInfo(HardwareCollector hardware) =>
        hardware.last_check_time.HasValue && 
        !string.IsNullOrEmpty(hardware.check_method) &&
        hardware.check_interval.HasValue &&
        hardware.is_active.HasValue;

    /// <summary>
    /// 檢查是否需要更新（根據更新時間間隔）
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>是否需要更新</returns>
    public static bool ShouldUpdate(HardwareCollector hardware) =>
        !hardware.updated_at.HasValue || !hardware.check_interval.HasValue ||
        (DateTime.UtcNow - hardware.updated_at.Value).TotalSeconds >= hardware.check_interval.Value;

    /// <summary>
    /// 計算檢查狀態
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>檢查狀態</returns>
    public static string CalculateCheckStatus(HardwareCollector hardware)
    {
        if (!hardware.last_check_time.HasValue)
            return "從未檢查";

        var timeSinceLastCheck = DateTime.UtcNow - hardware.last_check_time.Value;
        
        return timeSinceLastCheck.TotalMinutes < 1 ? "剛剛檢查" :
               timeSinceLastCheck.TotalMinutes < 60 ? $"{(int)timeSinceLastCheck.TotalMinutes} 分鐘前檢查" :
               timeSinceLastCheck.TotalHours < 24 ? $"{(int)timeSinceLastCheck.TotalHours} 小時前檢查" :
               $"{(int)timeSinceLastCheck.TotalDays} 天前檢查";
    }

    #endregion
}

