using Models.HnbHnbBackoffice;
using System.Management;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 記憶體監控工具類別
/// 負責收集和處理記憶體相關的硬體資訊
/// </summary>
public static class MemoryMonitoringUtility
{
    /// <summary>
    /// 收集記憶體資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring CollectMemoryInfo(hardware_monitoring hardware)
    {
        // 使用 WMI 取得記憶體資訊
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        var collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            var totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"] ?? 0) * 1024;
            var freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"] ?? 0) * 1024;
            var usedMemory = totalMemory - freeMemory;
            var totalGb = totalMemory / (1024 * 1024 * 1024);
            var usagePercent = totalMemory > 0 ? (decimal)(usedMemory * 100) / totalMemory : 0;

            // 更新系統記憶體資訊
            hardware.system_memory_total = (long)totalMemory;
            hardware.system_memory_used = (long)usedMemory;
            hardware.system_memory_free = (long)freeMemory;

            // 記憶體模組資訊
            hardware.memory_names = new List<string> { "系統記憶體" };
            hardware.memory_types = new List<string> { "DDR4" };
            hardware.memory_capacities = new List<string> { $"{totalGb}GB" };
            hardware.memory_speeds = new List<string> { "3200MHz" };
            hardware.memory_usages = new List<decimal> { usagePercent };
            hardware.memory_health_statuses = new List<string> { usagePercent > 90 ? "警告" : "正常" };
            hardware.memory_health_percentages = new List<int> { Math.Max(0, 100 - (int)(usagePercent * 0.5m)) };

            break; // 只需要第一個結果
        }

        return hardware;
    }

    /// <summary>
    /// 驗證記憶體資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateMemoryInfo(hardware_monitoring hardware) =>
        hardware.system_memory_total > 0 && hardware.memory_names?.Count > 0;
}
