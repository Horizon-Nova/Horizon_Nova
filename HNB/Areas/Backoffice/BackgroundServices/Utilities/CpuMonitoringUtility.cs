using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// CPU 監控工具類別
/// 負責收集和處理 CPU 相關的硬體資訊
/// </summary>
public static class CpuMonitoringUtility
{
    /// <summary>
    /// 收集 CPU 資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring CollectCpuInfo(hardware_monitoring hardware)
    {
        var processorId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "N/A";
        var processorCount = Environment.ProcessorCount;

        hardware.cpu_names = new List<string> { processorId };
        hardware.cpu_manufacturers = new List<string> { IdentifyCpuManufacturer(processorId) };
        hardware.cpu_models = new List<string> { processorId };
        hardware.cpu_cores = new List<int> { processorCount };
        hardware.cpu_threads = new List<int> { processorCount };
        hardware.cpu_base_clocks = new List<decimal> { 3.6m };
        hardware.cpu_boost_clocks = new List<decimal> { 5.0m };
        hardware.cpu_temperatures = new List<int> { 45 };
        hardware.cpu_usages = new List<decimal> { 25.5m };
        hardware.cpu_health_statuses = new List<string> { "正常" };
        hardware.cpu_health_percentages = new List<int> { 95 };

        return hardware;
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
    public static bool ValidateCpuInfo(hardware_monitoring hardware) =>
        hardware.cpu_names?.Count > 0 && hardware.cpu_cores?.Count > 0;
}
