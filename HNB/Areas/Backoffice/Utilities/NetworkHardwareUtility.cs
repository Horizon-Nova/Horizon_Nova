using HNB.Areas.Backoffice.Dtos;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Management;

namespace HNB.Areas.Backoffice.Utilities;

/// <summary>
/// 網路硬體工具類別
/// 負責收集和處理網路介面相關的硬體資訊
/// </summary>
public static class NetworkHardwareUtility
{
    /// <summary>
    /// 收集網路資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static HardwareCollector CollectNetworkInfo(HardwareCollector hardware)
    {
        var networkInterfaces = new List<string>();
        var networkTypes = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
            var collection = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                var name = obj["Name"]?.ToString() ?? "N/A";

                if (!name.Contains("Loopback") && !name.Contains("Microsoft"))
                {
                    networkInterfaces.Add(name);
                    networkTypes.Add(IdentifyNetworkType(name));
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in interfaces)
            {
                networkInterfaces.Add(ni.Name);
                networkTypes.Add(IdentifyNetworkType(ni.Name));
            }
        }

        if (networkInterfaces.Count == 0)
        {
            networkInterfaces.Add("未檢測到網路介面");
            networkTypes.Add("N/A");
        }

        hardware.network_interfaces = networkInterfaces;
        hardware.network_types = networkTypes;
        hardware.network_speeds = null;
        hardware.network_statuses = null;
        hardware.network_rx_bytes = null;
        hardware.network_tx_bytes = null;

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
    public static bool ValidateNetworkInfo(HardwareCollector hardware) =>
        hardware.network_interfaces?.Count > 0;
}

