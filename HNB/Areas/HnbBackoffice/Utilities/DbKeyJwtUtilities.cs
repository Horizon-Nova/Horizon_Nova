using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Models.HnbHnbBackoffice;
using System.Linq;

namespace HNB.Areas.HnbBackoffice.Utilities
{
    /// <summary>
    /// 以資料表 security_ip_key 的紀錄為依據，發行 HS256 JWT 並寫入 Cookie。
    /// 提供刪除 Cookie 與取得客戶端 IP 的工具函式。
    /// </summary>
    public class DbKeyJwtUtilities
    {
        #region 常數
        /// <summary>承載 JWT 的 Cookie 名稱。</summary>
        public const string TokenCookieName = "HNB_API_TOKEN";
        #endregion

        #region JWT 建立並寫入 Cookie
        /// <summary>
        /// 建立 JWT（HS256），並將 Token 以 Cookie 形式寫入回應；Cookie 的 Expires 與 JWT exp 對齊。
        /// </summary>
        /// <param name="ctx">HttpContext，用於取得請求資訊與寫入回應 Cookie。</param>
        /// <param name="cfg">IConfiguration，讀取 Jwt:Secret / Jwt:Issuer / Jwt:Audience / Jwt:KfpSalt。</param>
        /// <param name="row">該次會話對應的 security_ip_key 資料列（來源於資料庫）。</param>
        /// <param name="crossSite">
        /// 是否允許跨站（例如 iframe/不同子網域）傳遞 Cookie。
        /// true: SameSite=None（需 HTTPS）；false: SameSite=Lax。
        /// </param>
        /// <returns>回傳 (token, expiresAt)。</returns>
        public (string token, DateTimeOffset expiresAt) BuildJwtAndSetCookie(
            HttpContext ctx, IConfiguration cfg, security_ip_key row, bool crossSite = false)
        {
            if (row is null) throw new ArgumentNullException(nameof(row));

            var secret = cfg["Jwt:Secret"] ?? "change-me";
            var issuer = cfg["Jwt:Issuer"] ?? "HNB-Auth";
            var audience = cfg["Jwt:Audience"] ?? "HNB-Clients";
            var kfpSalt = cfg["Jwt:KfpSalt"] ?? "rotate-me-regularly";

            // 簽章憑證（HS256）
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256);

            static string ComputeFingerprint(string key, string salt)
            {
                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key + salt));
                var hex = Convert.ToHexString(bytes).ToLowerInvariant();
                return hex[..16];
            }
            var kfp = ComputeFingerprint(row.key ?? string.Empty, kfpSalt);

            // 將資料庫時間視為 UTC（保證 JWT 標準欄位時間一致性）
            static DateTime AssumeUtc(DateTime dt)
                => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            var nbfDb = AssumeUtc(row.created_at);
            var expDb = AssumeUtc(row.expires_at);
            var expDto = new DateTimeOffset(expDb);
            var iatUnix = new DateTimeOffset(nbfDb).ToUnixTimeSeconds();

            // 取得當前請求 IP
            var reqIp = GetClientIp(ctx).ToString();

            // 組裝 Claims（最小可辨識集合 + 風控欄位）
            var claims = new List<Claim>
            {
                // Sub: 指向授權主體（此處以 DB 中的 ip_addr 為主體）
                new(JwtRegisteredClaimNames.Sub, row.ip_addr?.ToString() ?? string.Empty),
                // Iat: 簽發時間（Unix 秒）
                new(JwtRegisteredClaimNames.Iat, iatUnix.ToString(), ClaimValueTypes.Integer64),
                // Jti: 唯一識別，便於追蹤與撤銷
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // 自定：kid（對應 DB 主鍵 id）
                new("kid",  row.id.ToString()),
                // 自定：kfp（key 的短指紋，避免外露原 key）
                new("kfp",  kfp),
                // 自定：tprm（時間參數，配合伺服端驗證策略）
                new("tprm", row.time_param.ToString()),
                // 自定：ip（簽發時請求端 IP）
                new("ip",   reqIp)
            };

            // 建立 JWT（含 nbf/exp），序列化為字串
            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: nbfDb,
                expires: expDb,
                signingCredentials: creds
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            // 寫入 Cookie（HttpOnly + Secure；跨站需 SameSite=None 並強制 HTTPS）
            var opts = new CookieOptions
            {
                Expires = expDto,           // 與 JWT exp 對齊，便於前後端行為一致
                HttpOnly = true,            // 防止前端 JS 讀取
                Secure = true,              // 僅在 HTTPS 傳輸
                Path = "/",                 // 全域路徑
                SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Lax
            };
            var domain = cfg["Jwt:CookieDomain"];
            if (!string.IsNullOrWhiteSpace(domain))
                opts.Domain = domain;
            if (crossSite)
                opts.Extensions.Add("Partitioned");

            ctx.Response.Cookies.Append(TokenCookieName, token, opts);

            ctx.Response.OnStarting(() =>
            {
                var sc = string.Join(" | ", ctx.Response.Headers["Set-Cookie"].ToArray());
                Console.WriteLine($"[Auth] Set-Cookie => {sc}");
                return Task.CompletedTask;
            });

            return (token, expDto);
        }
        #endregion

        #region 刪除 Cookie
        /// <summary>
        /// 以多組常見 Path 嘗試刪除同名 Cookie，盡可能清乾淨（避免殘留）。
        /// </summary>
        /// <param name="ctx">HttpContext。</param>
        /// <param name="crossSite">
        /// 是否以 SameSite=None 刪除（對應曾以跨站策略寫入的 Cookie）。
        /// </param>
        public void DeleteAuthCookieEverywhere(HttpContext ctx, bool crossSite = false)
        {
            var paths = new[] { "/", "/HnbBackoffice", "/HnbBackoffice/Authorize" };
            foreach (var p in paths)
            {
                ctx.Response.Cookies.Delete(TokenCookieName, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = p,
                    SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Lax
                });
            }

            ctx.Response.Cookies.Delete(TokenCookieName);
        }
        #endregion

        #region 工具：Key Fingerprint（類別層提供者，供需要時改為共用）
        /// <summary>
        /// 以 key + salt 作為輸入來源，產生 SHA256 並回傳 16 字元短指紋（十六進位小寫）。
        /// </summary>
        private static string ComputeFingerprint(string key, string salt)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key + salt));
            var hex = Convert.ToHexString(bytes).ToLowerInvariant();
            return hex[..16];
        }
        #endregion

        #region 工具：時間標記
        /// <summary>
        /// 若傳入時間非 UTC，則標記為 UTC（不更動數值，只調整 Kind）。
        /// </summary>
        private static DateTime EnsureUtc(DateTime dt)
            => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        #endregion

        #region 工具：取得客戶端 IP
        /// <summary>
        /// 取得請求端 IP：優先取 X-Forwarded-For（第一個）、再取 X-Real-IP，最後取 RemoteIpAddress。
        /// 同時將 IPv6 映射的 IPv4 轉回 IPv4 格式。
        /// </summary>
        /// <remarks>
        /// 請確保反向代理（如 Nginx/ALB）有正確設定並只信任可信來源的轉發標頭。
        /// </remarks>
        public static IPAddress GetClientIp(HttpContext ctx)
        {
            var hdr = ctx.Request.Headers;

            // X-Forwarded-For：可能有多個 IP，以第一個為原始客戶端 IP
            var xff = hdr["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var first = xff.Split(',')[0].Trim();
                if (IPAddress.TryParse(first, out var ip1))
                    return ip1.IsIPv4MappedToIPv6 ? ip1.MapToIPv4() : ip1;
            }

            // X-Real-IP：某些代理會提供單一原始 IP
            var xrip = hdr["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xrip) && IPAddress.TryParse(xrip, out var ip2))
                return ip2.IsIPv4MappedToIPv6 ? ip2.MapToIPv4() : ip2;

            // 回退至 Kestrel 連線端點
            var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6Loopback;
            return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
        }
        #endregion
    }
}
