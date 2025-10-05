using Microsoft.AspNetCore.Http;
using Models.HnbHnbBackoffice;
using HNB.Areas.Backoffice.BackgroundServices.Utilities;
using HNB.Areas.Backoffice.BackgroundServices.Repositories;
using System.Net;

namespace HNB.Areas.Backoffice.BackgroundServices.Middleware;

/// <summary>
/// 硬體監控中介軟體
/// 負責收集硬體資訊並儲存到資料庫
/// </summary>
public class HardwareMonitoringMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context)
    {
        // 檢查是否需要進行硬體監控
        if (ShouldPerformHardwareMonitoring(context))
        {
            await PerformHardwareMonitoringAsync(context);
        }

        // 繼續執行下一個中介軟體
        await _next(context);
    }

    /// <summary>
    /// 判斷是否需要執行硬體監控
    /// </summary>
    private static bool ShouldPerformHardwareMonitoring(HttpContext context)
    {
        // 只對特定的路由執行硬體監控
        var path = context.Request.Path.Value?.ToLower();
        
        // 排除靜態資源和 API 路由
        if (path != null && (
            path.StartsWith("/css/") ||
            path.StartsWith("/js/") ||
            path.StartsWith("/images/") ||
            path.StartsWith("/favicon.ico") ||
            path.StartsWith("/api/") ||
            path.Contains(".")))
        {
            return false;
        }

        // 只對後台管理系統的主要頁面執行監控
        return path != null && path.StartsWith("/backoffice/");
    }

    /// <summary>
    /// 執行硬體監控
    /// </summary>
    private async Task PerformHardwareMonitoringAsync(HttpContext context)
    {
        // 獲取客戶端 IP 位址
        var clientIp = GetClientIpAddress(context);
        
        // 檢查是否需要更新（避免過於頻繁的監控）
        if (!ShouldUpdateHardwareInfo(clientIp))
        {
            return;
        }

        // 使用 HardwareMonitoringUtility 收集完整的硬體資訊
        var hardwareInfo = HardwareMonitoringUtility.CollectCompleteHardwareInfo(clientIp, "middleware", 300);
        hardwareInfo.host_name = Environment.MachineName;
        hardwareInfo.last_check_time = DateTime.UtcNow;
        hardwareInfo.check_method = "middleware";
        hardwareInfo.check_interval = 300; // 5 分鐘間隔
        hardwareInfo.is_active = true;
        hardwareInfo.created_at = DateTime.UtcNow;
        hardwareInfo.updated_at = DateTime.UtcNow;

        // 驗證硬體資訊
        var validationResult = HardwareMonitoringUtility.ValidateHardwareInfo(hardwareInfo);
        if (validationResult.isValid)
        {
            // 使用 IServiceProvider 解析 Scoped 服務
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<HardwareMonitoringRepository>();
            
            // 儲存到資料庫
            var savedHardware = repository.InsertHardwareMonitoring(hardwareInfo);
        }
    }

    /// <summary>
    /// 獲取客戶端 IP 位址
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // 檢查是否有代理伺服器
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 直接連接的 IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    /// <summary>
    /// 判斷是否需要更新硬體資訊
    /// </summary>
    private bool ShouldUpdateHardwareInfo(string clientIp)
    {
        // 使用 IServiceProvider 解析 Scoped 服務
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<HardwareMonitoringRepository>();
        
        // 查詢現有的硬體監控記錄
        var existingHardware = repository.QueryHardwareMonitoringList()
            .FirstOrDefault(h => h.server_ip == clientIp);

        if (existingHardware == null)
        {
            // 沒有記錄，需要建立
            return true;
        }

        // 檢查最後更新時間（使用 updated_at 而不是 last_check_time）
        if (existingHardware.updated_at.HasValue)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - existingHardware.updated_at.Value;
            // 如果距離上次更新超過 5 分鐘，則需要更新
            return timeSinceLastUpdate.TotalMinutes >= 5;
        }

        // 沒有更新時間記錄，需要更新
        return true;
    }
}
