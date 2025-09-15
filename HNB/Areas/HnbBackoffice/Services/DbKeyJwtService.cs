using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Utilities;
using Models.HnbHnbBackoffice;
using System.Net;

namespace HNB.Areas.HnbBackoffice.Services;

public class DbKeyJwtService(DbKeyJwtRepositories rep, IConfiguration cfg, DbKeyJwtUtilities jwtUtil)
{
    /// <summary>建立或更新該 IP 的一筆紀錄，回傳儲存後的實體（包含 id）</summary>
    public security_ip_key SaveIp(HttpContext httpContext, string keyComponents, string? note = null)
    {
        var ipAddr = DbKeyJwtUtilities.GetClientIp(httpContext);
        var entity = new security_ip_key
        {
            ip_addr = ipAddr,
            key_components = keyComponents,
            note = note,
        };
        return rep.Save(entity);
    }

    /// <summary>登入後依該次儲存的同一筆 kid 產生 JWT，並掛 Cookie（內部由 Utilities 完成）。</summary>
    public (string token, DateTimeOffset expiresAt) IssueTokenAfterLogin(HttpContext ctx, string keyComponents, string? note = null, bool crossSite = true)
    {
        var row = SaveIp(ctx, keyComponents, note);
        row = rep.GetById(row.id);
        return jwtUtil.BuildJwtAndSetCookie(ctx, cfg, row!, crossSite);
    }

    /// <summary> 若你仍需要以 kid 再次簽發。</summary>
    public (string token, DateTimeOffset expiresAt) BuildJwtById(HttpContext ctx, long kid, bool crossSite = true)
    {
        var row = rep.GetById(kid) ?? throw new InvalidOperationException($"security_ip_keys not found: id={kid}");
        return jwtUtil.BuildJwtAndSetCookie(ctx, cfg, row, crossSite);
    }
}
