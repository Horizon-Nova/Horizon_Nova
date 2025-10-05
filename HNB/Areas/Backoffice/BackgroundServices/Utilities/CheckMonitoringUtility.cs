using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.BackgroundServices.Utilities;

/// <summary>
/// 檢查監控工具類別
/// 負責處理檢查相關的資訊和設定
/// </summary>
public static class CheckMonitoringUtility
{
    /// <summary>
    /// 設定檢查資訊並更新到硬體監控模型
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <param name="checkMethod">檢查方式</param>
    /// <param name="checkInterval">檢查間隔（秒）</param>
    /// <returns>更新後的硬體監控模型</returns>
    public static hardware_monitoring SetCheckInfo(hardware_monitoring hardware, string checkMethod = "agent", int checkInterval = 300)
    {
        // 設定檢查資訊
        hardware.last_check_time = DateTime.UtcNow;
        hardware.check_method = checkMethod ?? "agent";
        hardware.check_interval = checkInterval;
        hardware.is_active = true;

        // 設定時間戳記
        hardware.created_at ??= DateTime.UtcNow;
        hardware.updated_at = DateTime.UtcNow;

        return hardware;
    }

    /// <summary>
    /// 驗證檢查資訊
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>驗證結果</returns>
    public static bool ValidateCheckInfo(hardware_monitoring hardware) =>
        hardware.last_check_time.HasValue && 
        !string.IsNullOrEmpty(hardware.check_method) &&
        hardware.check_interval.HasValue &&
        hardware.is_active.HasValue;

    /// <summary>
    /// 檢查是否需要更新（根據更新時間間隔）
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>是否需要更新</returns>
    public static bool ShouldUpdate(hardware_monitoring hardware) =>
        !hardware.updated_at.HasValue || !hardware.check_interval.HasValue ||
        (DateTime.UtcNow - hardware.updated_at.Value).TotalSeconds >= hardware.check_interval.Value;

    /// <summary>
    /// 計算檢查狀態
    /// </summary>
    /// <param name="hardware">硬體監控模型</param>
    /// <returns>檢查狀態</returns>
    public static string CalculateCheckStatus(hardware_monitoring hardware)
    {
        if (!hardware.last_check_time.HasValue)
            return "從未檢查";

        var timeSinceLastCheck = DateTime.UtcNow - hardware.last_check_time.Value;
        
        return timeSinceLastCheck.TotalMinutes < 1 ? "剛剛檢查" :
               timeSinceLastCheck.TotalMinutes < 60 ? $"{(int)timeSinceLastCheck.TotalMinutes} 分鐘前檢查" :
               timeSinceLastCheck.TotalHours < 24 ? $"{(int)timeSinceLastCheck.TotalHours} 小時前檢查" :
               $"{(int)timeSinceLastCheck.TotalDays} 天前檢查";
    }
}
