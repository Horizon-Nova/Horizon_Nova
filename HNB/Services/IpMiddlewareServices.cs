using HNB.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Models.Hnb;

namespace HNB.Services;

/// <summary>
/// IP 中間件服務層，負責處理 IP 封鎖和速率限制的業務邏輯
/// </summary>
public class IpMiddlewareServices(BlockedIpRepository rep, IMemoryCache cache, ILogger<IpMiddlewareServices> logger, IHostEnvironment env)
{
    private const int LIMIT = 999999;
    private const int BLOCK_MINUTES = 10;

    /// <summary>
    /// 取得客戶端 IP 位址
    /// </summary>
    /// <param name="ctx">HTTP 上下文</param>
    /// <returns>IP 位址字串</returns>
    public string LoadClientIp(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwarded)
               ? forwarded.Split(',')[0].Trim()
               : ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }

    /// <summary>
    /// 檢查 IP 是否被封鎖或需要封鎖
    /// </summary>
    /// <param name="ip">IP 位址</param>
    /// <returns>
    /// isBlocked: IP 是否已被封鎖
    /// shouldBlockNow: 是否應該立即封鎖此 IP
    /// </returns>
    public (bool isBlocked, bool shouldBlockNow) Check(string ip)
    {
        var isBlocked = cache.GetOrCreate($"blocked:{ip}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return rep.QueryIsBlocked(ip);
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
            BlockIp(ip, "Rate limit exceeded");
            return (false, true);
        }

        return (false, false);
    }

    /// <summary>
    /// 封鎖指定 IP
    /// </summary>
    /// <param name="ip">IP 位址</param>
    /// <param name="reason">封鎖原因</param>
    /// <returns>成功返回 true，失敗返回 false</returns>
    private bool BlockIp(string ip, string reason)
    {
        var blockedIp = new blocked_ip
        {
            id = Guid.NewGuid(),
            ip_address = ip,
            reason = reason
        };

        var result = rep.Insert(blockedIp);

        if (result)
        {
            cache.Set($"blocked:{ip}", true, TimeSpan.FromHours(1));
            logger.LogWarning($"IP {ip} 被封鎖（{reason}）。");
        }

        return result;
    }

    /// <summary>
    /// 載入封鎖頁面的 HTML 內容
    /// </summary>
    /// <returns>HTML 字串</returns>
    public string LoadBlockHtml()
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

