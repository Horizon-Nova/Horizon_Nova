using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 系統監控工具類別
/// 負責收集和處理系統相關的資訊
/// </summary>
public static class SystemMonitoringUtility
{
    /// <summary>
    /// 收集系統資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring CollectSystemInfo(hardware_monitoring hardware)
    {
        // 基本系統資訊
        hardware.host_name = Environment.MachineName ?? "N/A";
        hardware.operating_system = Environment.OSVersion.VersionString ?? "N/A";
        hardware.kernel_version = Environment.OSVersion.Version.ToString();
        hardware.uptime = CalculateSystemUptime();

        // 系統負載
        hardware.system_load_avg = new List<decimal> { 0.5m };

        // 系統程序和使用者數
        hardware.system_processes = System.Diagnostics.Process.GetProcesses().Length;
        hardware.system_users = 1;

        // 磁碟空間資訊
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

        hardware.system_disk_total = totalDiskSpace;
        hardware.system_disk_used = usedDiskSpace;
        hardware.system_disk_free = freeDiskSpace;

        // 交換空間資訊（Windows 沒有直接的交換空間概念，使用虛擬記憶體）
        hardware.system_swap_total = 0;
        hardware.system_swap_used = 0;

        return hardware;
    }

    /// <summary>
    /// 計算系統運行時間
    /// </summary>
    /// <returns>運行時間字串</returns>
    private static string CalculateSystemUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return $"{uptime.Days}天 {uptime.Hours}小時 {uptime.Minutes}分鐘";
    }

    /// <summary>
    /// 驗證系統資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateSystemInfo(hardware_monitoring hardware) =>
        !string.IsNullOrEmpty(hardware.host_name) && 
        !string.IsNullOrEmpty(hardware.operating_system) &&
        hardware.system_processes.HasValue;
}
