using HNB.Models;
using Microsoft.EntityFrameworkCore;

namespace HNB.Filters;

public class BlockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BlockMiddleware> _logger;

    public BlockMiddleware(RequestDelegate next, ILogger<BlockMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RailwayContext db)
    {
        var ip = GetClientIp(context);

        // 檢查是否在黑名單，且未過期
        var blocked = await db.BlockedIps
            .Where(b => b.Ip == ip && (b.ExpiresAt == null || b.ExpiresAt > DateTimeOffset.UtcNow))
            .AnyAsync();

        if (blocked)
        {
            _logger.LogWarning($"封鎖請求：IP={ip}");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("您的 IP 已被封鎖。");
            return;
        }

        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }
}
