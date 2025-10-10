using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 硬體監控主要工具類別
/// 整合所有硬體監控 Utilities，提供統一的硬體資訊收集介面
/// </summary>
public static class HardwareMonitoringUtility
{
    /// <summary>
    /// 收集完整的硬體監控資訊
    /// </summary>
    /// <param name="serverIp">伺服器IP（用於定位現有記錄）</param>
    /// <param name="checkMethod">檢查方式</param>
    /// <param name="checkInterval">檢查間隔（秒）</param>
    /// <returns>完整的硬體監控模型</returns>
    public static hardware_monitoring CollectCompleteHardwareInfo(string serverIp, string checkMethod = "agent", int checkInterval = 300)
    {
        var hardware = new hardware_monitoring
        {
            server_ip = serverIp,
            server_location = "台灣",
            server_provider = "自建",
            environment_type = "Production"
        };

        try
        {
            hardware = CpuMonitoringUtility.CollectCpuInfo(hardware);
            hardware = GpuMonitoringUtility.CollectGpuInfo(hardware);
            hardware = MemoryMonitoringUtility.CollectMemoryInfo(hardware);
            hardware = StorageMonitoringUtility.CollectStorageInfo(hardware);
            hardware = NetworkMonitoringUtility.CollectNetworkInfo(hardware);
            hardware = SystemMonitoringUtility.CollectSystemInfo(hardware);
            hardware = PowerMonitoringUtility.CollectPowerInfo(hardware);
            hardware = CheckMonitoringUtility.SetCheckInfo(hardware, checkMethod, checkInterval);

            Console.WriteLine($"硬體監控資訊收集完成 - 伺服器: {serverIp}");
            return hardware;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"收集硬體監控資訊時發生錯誤: {ex.Message}");
            hardware = CheckMonitoringUtility.SetCheckInfo(hardware, checkMethod, checkInterval);
            return hardware;
        }
    }

    /// <summary>
    /// 驗證硬體監控資訊的完整性
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果和錯誤訊息</returns>
    public static (bool isValid, List<string> errors) ValidateHardwareInfo(hardware_monitoring hardware)
    {
        var errors = new List<string>();

        if (!CpuMonitoringUtility.ValidateCpuInfo(hardware))
            errors.Add("CPU 資訊驗證失敗");

        if (!GpuMonitoringUtility.ValidateGpuInfo(hardware))
            errors.Add("GPU 資訊驗證失敗");

        if (!MemoryMonitoringUtility.ValidateMemoryInfo(hardware))
            errors.Add("記憶體資訊驗證失敗");

        if (!StorageMonitoringUtility.ValidateStorageInfo(hardware))
            errors.Add("儲存資訊驗證失敗");

        if (!NetworkMonitoringUtility.ValidateNetworkInfo(hardware))
            errors.Add("網路資訊驗證失敗");

        if (!SystemMonitoringUtility.ValidateSystemInfo(hardware))
            errors.Add("系統資訊驗證失敗");

        if (!PowerMonitoringUtility.ValidatePowerInfo(hardware))
            errors.Add("電源資訊驗證失敗");

        if (!CheckMonitoringUtility.ValidateCheckInfo(hardware))
            errors.Add("檢查資訊驗證失敗");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// 生成硬體監控摘要資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>摘要資訊</returns>
    public static string GenerateHardwareSummary(hardware_monitoring hardware)
    {
        var summary = new List<string>();

        if (hardware.cpu_names?.Count > 0)
        {
            summary.Add($"CPU: {hardware.cpu_names[0]} ({hardware.cpu_cores?[0]} 核心)");
        }

        if (hardware.gpu_names?.Count > 0 && hardware.gpu_names[0] != "未檢測到獨立顯卡")
        {
            summary.Add($"GPU: {hardware.gpu_names[0]}");
        }

        if (hardware.system_memory_total.HasValue)
        {
            var memoryGB = hardware.system_memory_total.Value / (1024 * 1024 * 1024);
            summary.Add($"記憶體: {memoryGB}GB");
        }

        if (hardware.storage_names?.Count > 0)
        {
            summary.Add($"儲存: {hardware.storage_names.Count} 個裝置");
        }

        if (hardware.network_interfaces?.Count > 0)
        {
            summary.Add($"網路: {hardware.network_interfaces.Count} 個介面");
        }

        return string.Join(", ", summary);
    }

    /// <summary>
    /// 評估硬體健康狀態
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>健康狀態</returns>
    public static string EvaluateHardwareHealthStatus(hardware_monitoring hardware)
    {
        var issues = new List<string>();

        if (hardware.cpu_temperatures?.Count > 0)
        {
            var maxCpuTemp = hardware.cpu_temperatures.Max();
            if (maxCpuTemp > 80)
                issues.Add($"CPU 溫度過高: {maxCpuTemp}°C");
        }

        if (hardware.gpu_temperatures?.Count > 0)
        {
            var maxGpuTemp = hardware.gpu_temperatures.Max();
            if (maxGpuTemp > 85)
                issues.Add($"GPU 溫度過高: {maxGpuTemp}°C");
        }

        if (hardware.memory_usages?.Count > 0)
        {
            var maxMemoryUsage = hardware.memory_usages.Max();
            if (maxMemoryUsage > 90)
                issues.Add($"記憶體使用率過高: {maxMemoryUsage:F1}%");
        }

        if (hardware.system_disk_total.HasValue && hardware.system_disk_free.HasValue)
        {
            var diskUsagePercent = (decimal)(hardware.system_disk_total.Value - hardware.system_disk_free.Value) * 100 / hardware.system_disk_total.Value;
            if (diskUsagePercent > 90)
                issues.Add($"磁碟空間不足: {diskUsagePercent:F1}% 已使用");
        }

        if (issues.Count == 0)
            return "健康";
        else
            return $"警告: {string.Join(", ", issues)}";
    }
}
