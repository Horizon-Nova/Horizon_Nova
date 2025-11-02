using HNB.IIS.Core.Models.Hnbdata;
using HNB.IIS.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HNB.IIS.Core.Services;

public class IpMiddlewareServices(BlockedIpRepository repo, IMemoryCache cache, ILogger<IpMiddlewareServices> logger, IHostEnvironment env)
{
    private const int LIMIT = 999999;
    private const int BLOCK_MINUTES = 10;

    public string LoadClientIp(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwarded)
               ? forwarded.Split(',')[0].Trim()
               : ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }

    public (bool isBlocked, bool shouldBlockNow) CheckIp(string ip)
    {
        var isBlocked = cache.GetOrCreate($"blocked:{ip}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return repo.QueryIsBlocked(ip);
        })!;

        if (isBlocked) return (true, false);

        var hits = cache.GetOrCreate($"hits:{ip}", e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        })!;

        hits = hits + 1;
        cache.Set($"hits:{ip}", hits, TimeSpan.FromMinutes(1));

        if (hits > LIMIT)
        {
            CreateBlockedIp(ip, "Rate limit exceeded");
            return (false, true);
        }

        return (false, false);
    }

    private bool CreateBlockedIp(string ip, string reason)
    {
        var now = DateTime.UtcNow;

        var blockedIp = new blocked_ip
        {
            ip = ip,
            reason = reason,
            created_at = now,
            expires_at = now.AddMinutes(BLOCK_MINUTES)
        };

        var result = repo.InsertBlockedIp(blockedIp);

        if (result)
        {
            cache.Set($"blocked:{ip}", true, TimeSpan.FromHours(1));
            logger.LogWarning($"IP {ip} 被封鎖（{reason}）。");
        }

        return result;
    }

    public string LoadBlockHtml() => 
        cache.GetOrCreate("block:html", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var file = Path.Combine(env.ContentRootPath, "wwwroot", "ip-block.html");
            return File.Exists(file) ? File.ReadAllText(file) : "<h1>Access denied</h1>";
        })!;
}

