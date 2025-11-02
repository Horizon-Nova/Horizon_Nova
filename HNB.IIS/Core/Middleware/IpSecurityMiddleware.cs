using HNB.IIS.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HNB.IIS.Core.Middleware;

public class IpSecurityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, IpMiddlewareServices svc)
    {
        var ip = svc.LoadClientIp(ctx);
        var (blocked, shouldBlock) = svc.CheckIp(ip);

        if (blocked)
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(svc.LoadBlockHtml());
            return;
        }

        if (shouldBlock)
        {
            ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.Response.ContentType = "text/plain; charset=utf-8";
            await ctx.Response.WriteAsync("Too many requests: IP temporarily blocked.");
            return;
        }

        await next(ctx);
    }
}

