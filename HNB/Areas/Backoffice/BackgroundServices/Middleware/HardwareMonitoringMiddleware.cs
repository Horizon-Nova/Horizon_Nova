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
        if (ShouldPerformHardwareMonitoring(context))
        {
            await PerformHardwareMonitoringAsync(context);
        }

        await _next(context);
    }

    /// <summary>
    /// 判斷是否需要執行硬體監控
    /// </summary>
    private static bool ShouldPerformHardwareMonitoring(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        
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

        return path != null && path.StartsWith("/backoffice/");
    }

    /// <summary>
    /// 執行硬體監控
    /// </summary>
    private async Task PerformHardwareMonitoringAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        if (!ShouldUpdateHardwareInfo(clientIp))
        {
            return;
        }

        var hardwareInfo = HardwareMonitoringUtility.CollectCompleteHardwareInfo(clientIp, "middleware", 300);
        hardwareInfo.host_name = Environment.MachineName;
        hardwareInfo.last_check_time = DateTime.UtcNow;
        hardwareInfo.check_method = "middleware";
        hardwareInfo.check_interval = 300; // 5 分鐘間隔
        hardwareInfo.is_active = true;
        hardwareInfo.created_at = DateTime.UtcNow;
        hardwareInfo.updated_at = DateTime.UtcNow;

        var validationResult = HardwareMonitoringUtility.ValidateHardwareInfo(hardwareInfo);
        if (validationResult.isValid)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<HardwareMonitoringRepository>();
            
            var savedHardware = repository.InsertHardwareMonitoring(hardwareInfo);
        }
    }

    /// <summary>
    /// 獲取客戶端 IP 位址
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
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

        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    /// <summary>
    /// 判斷是否需要更新硬體資訊
    /// </summary>
    private bool ShouldUpdateHardwareInfo(string clientIp)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<HardwareMonitoringRepository>();
        
        var existingHardware = repository.QueryHardwareMonitoringList()
            .FirstOrDefault(h => h.server_ip == clientIp);

        if (existingHardware == null)
        {
            return true;
        }

        if (existingHardware.updated_at.HasValue)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - existingHardware.updated_at.Value;
            return timeSinceLastUpdate.TotalMinutes >= 5;
        }

        return true;
    }
}
