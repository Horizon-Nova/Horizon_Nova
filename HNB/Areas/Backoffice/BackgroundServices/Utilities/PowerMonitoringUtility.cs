using Models.HnbHnbBackoffice;
using System.Management;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 電源監控工具類別
/// 負責收集和處理電源相關的硬體資訊
/// </summary>
public static class PowerMonitoringUtility
{
    /// <summary>
    /// 收集電源資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring CollectPowerInfo(hardware_monitoring hardware)
    {
        // 預設值
        hardware.battery_level = 100; // 桌面電腦預設值
        hardware.power_efficiency = "高效";
        hardware.power_supply_info = "AC 電源 - 750W 80+ Gold";

        // 使用 WMI 取得電池資訊
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
        var collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            var batteryLevel = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0);
            var batteryStatus = obj["BatteryStatus"]?.ToString() ?? "N/A";

            if (batteryLevel > 0)
            {
                hardware.battery_level = batteryLevel;
                hardware.power_efficiency = batteryStatus;
            }

            break; // 只需要第一個結果
        }

        // 取得電源供應器資訊
        using var psuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");
        var psuCollection = psuSearcher.Get();

        foreach (ManagementObject obj in psuCollection)
        {
            var description = obj["Description"]?.ToString() ?? "N/A";
            if (!string.IsNullOrEmpty(description) && description != "N/A")
            {
                hardware.power_supply_info = description;
            }
            break; // 只需要第一個結果
        }

        return hardware;
    }

    /// <summary>
    /// 驗證電源資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidatePowerInfo(hardware_monitoring hardware) =>
        hardware.battery_level.HasValue && 
        !string.IsNullOrEmpty(hardware.power_efficiency) &&
        !string.IsNullOrEmpty(hardware.power_supply_info);
}
