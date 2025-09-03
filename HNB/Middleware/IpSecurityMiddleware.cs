using HNB.Services;

namespace HNB.Middleware;

/// <summary>
/// 極薄的一層：只做攔截與回應，所有邏輯都丟給 IpMiddlewareServices。
/// </summary>
public class IpSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpSecurityMiddleware> _logger;

    public IpSecurityMiddleware(RequestDelegate next, ILogger<IpSecurityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx, IpMiddlewareServices svc)
    {
        var ip = svc.GetClientIp(ctx);
        var (blocked, shouldBlock) = await svc.CheckAsync(ip);

        if (blocked)
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(svc.GetBlockHtml());
            return;
        }

        if (shouldBlock)
        {
            ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.Response.ContentType = "text/plain; charset=utf-8";
            await ctx.Response.WriteAsync("Too many requests: IP temporarily blocked.");
            return;
        }

        await _next(ctx);
    }

}
