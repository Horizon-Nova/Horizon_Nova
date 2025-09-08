using HNB.Areas.HnbBackoffice.Repositories;
using Microsoft.IdentityModel.Tokens;
using Models.HnbHnbBackoffice;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.HnbBackoffice.Services;

public class DbKeyJwtService(DbKeyJwtRepositories rep, IConfiguration cfg)
{
    #region 取得用戶端 IP
    /// <summary>取得用戶端 IP（含 X-Forwarded-For 與 X-Real-IP 支援）</summary>
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

    #region 儲存 IP Key（回傳該筆資料）
    /// <summary>建立或更新該 IP 的一筆紀錄，回傳儲存後的實體（包含 id）</summary>
    public async Task<security_ip_key> SaveIpAsync(HttpContext httpContext, string keyComponents, string? note = null, CancellationToken ct = default)
    {
        var ipAddr = GetClientIp(httpContext);
        var entity = new security_ip_key
        {
            ip_addr = ipAddr,
            key_components = keyComponents,
            note = note,
        };
        var saved = await rep.SaveAsync(entity, ct);
        return saved;
    }
    #endregion

    #region 登入後發 Token（固定以同一 kid 產生）
    /// <summary>登入後依該次儲存的同一筆 kid 產生 JWT，避免用 IP 取「最新一筆」造成對不上</summary>
    public async Task<(string token, DateTimeOffset expiresAt)> IssueTokenAfterLoginAsync(HttpContext ctx, string keyComponents, string? note = null, CancellationToken ct = default)
    {
        var row = await SaveIpAsync(ctx, keyComponents, note, ct);
        row = rep.GetById(row.id);

        var (token, exp) = BuildJwtById(row.id, GetClientIp(ctx));
        return (token, exp);
    }
    #endregion

    #region 依 kid 產生 JWT
    /// <summary>用同一筆 kid 產生 HS256 JWT；kfp = first16hex( SHA256( key + salt ) )</summary>
    public (string token, DateTimeOffset expiresAt) BuildJwtById(long kid, IPAddress reqIp)
    {
        var row = rep.GetById(kid);
        if (row == null) throw new InvalidOperationException($"security_ip_keys not found: id={kid}");

        var secret = cfg["Jwt:Secret"] ?? "change-me";
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);

        var salt = cfg["Jwt:KfpSalt"] ?? "rotate-me-regularly";
        static string ComputeFingerprint(string k, string s)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(k + s));
            var hex = Convert.ToHexString(bytes).ToLowerInvariant();
            return hex[..16];
        }

        var key = row.key ?? string.Empty;
        var kfp = ComputeFingerprint(key, salt);
        var nbf = row.created_at;
        var exp = row.expires_at;
        var iat = new DateTimeOffset(nbf).ToUnixTimeSeconds();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, row.ip_addr?.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("kid",  row.id.ToString()),
            new("kfp",  kfp),
            new("tprm", row.time_param.ToString()),
            new("ip",   reqIp.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"] ?? "HNB-Auth",
            audience: cfg["Jwt:Audience"] ?? "HNB-Clients",
            claims: claims,
            notBefore: nbf,
            expires: exp,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), new DateTimeOffset(exp));
    }
    #endregion
}
