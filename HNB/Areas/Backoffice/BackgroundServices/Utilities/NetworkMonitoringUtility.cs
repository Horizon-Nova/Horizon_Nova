using Models.HnbHnbBackoffice;
using System.Management;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 網路監控工具類別
/// 負責收集和處理網路介面相關的硬體資訊
/// </summary>
public static class NetworkMonitoringUtility
{
    /// <summary>
    /// 收集網路資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring CollectNetworkInfo(hardware_monitoring hardware)
    {
        var networkInterfaces = new List<string>();
        var networkTypes = new List<string>();
        var networkSpeeds = new List<string>();
        var networkStatuses = new List<string>();
        var networkRxBytes = new List<long>();
        var networkTxBytes = new List<long>();

        // 使用 WMI 取得網路介面資訊
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
        var collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            var name = obj["Name"]?.ToString() ?? "N/A";

            // 跳過迴環介面
            if (!name.Contains("Loopback") && !name.Contains("Microsoft"))
            {
                networkInterfaces.Add(name);
                networkTypes.Add(IdentifyNetworkType(name));
                networkSpeeds.Add("1Gbps");
                networkStatuses.Add("已連接");
                networkRxBytes.Add(0);
                networkTxBytes.Add(0);
            }
        }

        // 如果沒有找到網路介面，添加預設值
        if (networkInterfaces.Count == 0)
        {
            networkInterfaces.Add("未檢測到網路介面");
            networkTypes.Add("N/A");
            networkSpeeds.Add("N/A");
            networkStatuses.Add("未連接");
            networkRxBytes.Add(0);
            networkTxBytes.Add(0);
        }

        // 更新到模型
        hardware.network_interfaces = networkInterfaces;
        hardware.network_types = networkTypes;
        hardware.network_speeds = networkSpeeds;
        hardware.network_statuses = networkStatuses;
        hardware.network_rx_bytes = networkRxBytes;
        hardware.network_tx_bytes = networkTxBytes;

        return hardware;
    }

    /// <summary>
    /// 識別網路類型
    /// </summary>
    /// <param name="interfaceName">介面名稱</param>
    /// <returns>網路類型</returns>
    private static string IdentifyNetworkType(string interfaceName) =>
        interfaceName.Contains("WiFi", StringComparison.OrdinalIgnoreCase) || 
        interfaceName.Contains("Wireless", StringComparison.OrdinalIgnoreCase) ? "WiFi" :
        interfaceName.Contains("Ethernet", StringComparison.OrdinalIgnoreCase) || 
        interfaceName.Contains("LAN", StringComparison.OrdinalIgnoreCase) ? "Ethernet" : "N/A";

    /// <summary>
    /// 驗證網路資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateNetworkInfo(hardware_monitoring hardware) =>
        hardware.network_interfaces?.Count > 0;
}
