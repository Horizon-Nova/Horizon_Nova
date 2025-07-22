using HNB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HNB.Utilities;

/// <summary>
/// 封裝所有與 IP 安全／Rate-limit／黑名單 相關的商業邏輯與資料存取。
/// </summary>
public class IpMiddlewareServices
{
    private const int LIMIT = 100;          // 1 分鐘內請求上限
    private const int BLOCK_MINUTES = 10;   // 違規後封鎖時間

    private readonly HnbdataContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IpMiddlewareServices> _logger;
    private readonly IHostEnvironment _env;

    public IpMiddlewareServices(HnbdataContext db,IMemoryCache cache,ILogger<IpMiddlewareServices> logger, IHostEnvironment env)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
        _env = env;
    }

    /* ---------- 對 Middleware 暴露的 API ---------- */
    public string GetClientIp(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwarded)
               ? forwarded.Split(',')[0].Trim()
               : ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }

    /// <summary>
    /// 回傳 (isBlocked, shouldBlockNow)。<br/>
    /// 1. 若本來就在黑名單 → isBlocked=true。<br/>
    /// 2. 若 1 分鐘內 > LIMIT，服務層會寫入封鎖，回 shouldBlockNow=true。
    /// </summary>
    public async Task<(bool isBlocked, bool shouldBlockNow)> CheckAsync(string ip)
    {
        /* 先查黑名單（快取一小時） */
        var isBlocked = await _cache.GetOrCreateAsync($"blocked:{ip}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await _db.BlockedIps
                .AnyAsync(b => b.Ip == ip &&
                               (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow));
        });

        if (isBlocked) return (true, false);

        /* 1 分鐘內計數（只用 MemoryCache，不落 DB） */
        var hits = _cache.GetOrCreate($"hits:{ip}", e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        })!;

        hits = (int)hits + 1;
        _cache.Set($"hits:{ip}", hits, TimeSpan.FromMinutes(1));

        if ((int)hits > LIMIT)
        {
            await BlockIpAsync(ip, "Rate limit exceeded");
            return (false, true);
        }

        return (false, false);
    }

    /* ---------- 私有 Helper ---------- */
    private async Task BlockIpAsync(string ip, string reason)
    {
        var now = DateTime.UtcNow;

        _db.BlockedIps.Add(new BlockedIp
        {
            Ip = ip,
            Reason = reason,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(BLOCK_MINUTES)
        });
        await _db.SaveChangesAsync();

        _cache.Set($"blocked:{ip}", true, TimeSpan.FromHours(1));

        _logger.LogWarning($"IP {ip} 被封鎖（{reason}）。");
    }

    /// <summary>
    /// 讀取並快取 html
    /// </summary>
    public string GetBlockHtml()
    {
        return _cache.GetOrCreate("block:html", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var file = Path.Combine(_env.ContentRootPath, "wwwroot", "ip-block.html");
            return File.Exists(file)
                 ? File.ReadAllText(file)
                 : "<h1>Access denied</h1>";
        })!;
    }
}
