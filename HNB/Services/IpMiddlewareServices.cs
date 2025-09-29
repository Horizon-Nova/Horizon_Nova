using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Hnbdata;

namespace HNB.Services;

public class IpMiddlewareServices(HnbdataDbContext db, IMemoryCache cache, ILogger<IpMiddlewareServices> logger, IHostEnvironment env)
{
    private const int LIMIT = 999999;
    private const int BLOCK_MINUTES = 10;

    public string GetClientIp(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwarded)
               ? forwarded.Split(',')[0].Trim()
               : ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }

    public async Task<(bool isBlocked, bool shouldBlockNow)> CheckAsync(string ip)
    {
        var isBlocked = await cache.GetOrCreateAsync($"blocked:{ip}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await db.blocked_ips
                .AnyAsync(b => b.ip == ip &&
                               (b.expires_at == null || b.expires_at > DateTime.UtcNow));
        });

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
            await BlockIpAsync(ip, "Rate limit exceeded");
            return (false, true);
        }

        return (false, false);
    }

    private async Task BlockIpAsync(string ip, string reason)
    {
        var now = DateTime.UtcNow;

        db.blocked_ips.Add(new blocked_ip
        {
            ip = ip,
            reason = reason,
            created_at = now,
            expires_at = now.AddMinutes(BLOCK_MINUTES)
        });
        await db.SaveChangesAsync();

        cache.Set($"blocked:{ip}", true, TimeSpan.FromHours(1));

        logger.LogWarning($"IP {ip} 被封鎖（{reason}）。");
    }

    public string GetBlockHtml()
    {
        return cache.GetOrCreate("block:html", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var file = Path.Combine(env.ContentRootPath, "wwwroot", "ip-block.html");
            return File.Exists(file)
                 ? File.ReadAllText(file)
                 : "<h1>Access denied</h1>";
        })!;
    }
}
