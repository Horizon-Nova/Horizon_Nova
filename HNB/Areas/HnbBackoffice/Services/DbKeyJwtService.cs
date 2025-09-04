using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Utilities;
using Microsoft.IdentityModel.Tokens;
using Models.HnbHnbBackoffice;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace HNB.Areas.HnbBackoffice.Services;

public class DbKeyJwtService(DbKeyJwtRepositories rep ,IConfiguration cfg)
{

    public async Task<security_ip_key> SaveIpAsync(HttpContext httpContext,string keyComponents,string? note = null,CancellationToken ct = default)
    {
        var ipAddr = GetClientIp(httpContext);

        var entity = new security_ip_key
        {
            ip_addr = ipAddr,
            key_components = keyComponents,
            note = note,
        };

        return await rep.SaveAsync(entity, ct);
    }

    public async Task<(string token, DateTimeOffset expiresAt)> IssueTokenAfterLoginAsync(
    HttpContext ctx, string keyComponents, string? note = null,
    CancellationToken ct = default)
    {
        await SaveIpAsync(ctx, keyComponents, note, ct);

        var (token, exp) = BuildJwt(ctx);

        return (token, exp);
    }

    /// <summary>
    /// 用資料庫這筆 security_ip_key 產生 HS256 JWT（不碰 Program，不需要 DI 設定）。
    /// </summary>
    public (string token, DateTimeOffset expiresAt) BuildJwt(HttpContext http)
    {
        string subject = null;
        var ip = GetClientIp(http);
        var row = rep.SecurityIpKeyQuery(ip);

        var secret = cfg["Jwt:Secret"] ?? "change-me";
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);

        var handler = new JwtSecurityTokenHandler();

        var key = row.key ?? string.Empty;
        var kfp = key.Length > 16 ? key[..16] : key;

        var nbf = row.created_at!.Value.ToUniversalTime();
        var exp = row.expires_at!.Value.ToUniversalTime();

        var iat = new DateTimeOffset(nbf).ToUnixTimeSeconds();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, string.IsNullOrWhiteSpace(subject) ? (row.ip_addr?.ToString() ?? string.Empty) : subject),
            new(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("kid",  row.id.ToString()),
            new("kfp",  kfp),
            new("tprm", row.time_param.ToString()),
            new("ip",   ip.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"] ?? "HNB-Auth",
            audience: cfg["Jwt:Audience"] ?? "HNB-Clients",
            claims: claims,
            notBefore: nbf,
            expires: exp,
            signingCredentials: creds
        );

        return (handler.WriteToken(token), new DateTimeOffset(exp));
    }
    #region 私有
    private static IPAddress GetClientIp(HttpContext ctx)
    {
        var hdr = ctx.Request.Headers;

        var xff = hdr["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xff))
        {
            var first = xff.Split(',')[0].Trim();
            if (IPAddress.TryParse(first, out var ip1))
                return ip1.IsIPv4MappedToIPv6 ? ip1.MapToIPv4() : ip1;
        }

        var xrip = hdr["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xrip) && IPAddress.TryParse(xrip, out var ip2))
            return ip2.IsIPv4MappedToIPv6 ? ip2.MapToIPv4() : ip2;

        var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6Loopback;
        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
    }


    #endregion
}
