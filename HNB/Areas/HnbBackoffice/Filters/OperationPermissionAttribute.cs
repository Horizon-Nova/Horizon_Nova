using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HNB.Areas.HnbBackoffice.Filters;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Filters;

public sealed class OperationPermissionAttribute : TypeFilterAttribute
{
    public OperationPermissionAttribute(bool requireIpMatch = true, bool verifyDb = false)
        : base(typeof(OperationPermissionFilter))
    {
        Arguments = new object[] { requireIpMatch, verifyDb };
    }
}

public sealed class OperationPermissionFilter : IAsyncAuthorizationFilter
{
    private readonly IConfiguration _cfg;
    private readonly bool _requireIpMatch;
    private readonly bool _verifyDb;

    public OperationPermissionFilter(IConfiguration cfg, bool requireIpMatch, bool verifyDb)
    {
        _cfg = cfg;
        _requireIpMatch = requireIpMatch;
        _verifyDb = verifyDb;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;

        if (!http.Request.Cookies.TryGetValue("HNB_API_TOKEN", out var token) || string.IsNullOrWhiteSpace(token))
        {
            RedirectToLogin(context);
            return;
        }

        var secret = _cfg["Jwt:Secret"];
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            RedirectToLogin(context);
            return;
        }

        var handler = new JwtSecurityTokenHandler();
        var param = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };

        ClaimsPrincipal principal;
        JwtSecurityToken? jwt;
        try
        {
            principal = handler.ValidateToken(token, param, out var validated);
            jwt = validated as JwtSecurityToken;
            if (jwt == null || !string.Equals(jwt.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                RedirectToLogin(context);
                return;
            }
        }
        catch
        {
            RedirectToLogin(context);
            return;
        }

        if (_requireIpMatch)
        {
            var tokenIp = principal.FindFirst("ip")?.Value;
            var reqIp = GetClientIp(http);
            if (!string.IsNullOrEmpty(tokenIp) &&
                !string.Equals(tokenIp, reqIp, StringComparison.OrdinalIgnoreCase))
            {
                RedirectToLogin(context);
                return;
            }
        }

        #region verifyDb（IP + kid + kfp）
        if (_verifyDb)
        {


        }
        #endregion



    }

    private static void RedirectToLogin(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;

        foreach (var path in new[] { "/", "/HnbBackoffice" })
        {
            http.Response.Cookies.Delete("HNB_API_TOKEN", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = path
            });
        }

        var isAjax = IsAjaxRequest(http);
        if (isAjax)
        {
            context.Result = new JsonResult(new { ok = false })
            { StatusCode = StatusCodes.Status404NotFound };
            return;
        }
        context.Result = new RedirectToActionResult("Login", "Authorize", new RouteValueDictionary(new { area = "HnbBackoffice" }));
    }
    private static bool IsAjaxRequest(HttpContext http)
    {
        var xrw = http.Request.Headers["X-Requested-With"].ToString();
        if (!string.IsNullOrEmpty(xrw) && xrw.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return true;
        var accept = http.Request.Headers["Accept"].ToString() ?? "";
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIp(HttpContext ctx)
    {
        var xff = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xff) && IPAddress.TryParse(xff.Split(',')[0].Trim(), out var ip1))
            return (ip1.IsIPv4MappedToIPv6 ? ip1.MapToIPv4() : ip1).ToString();

        var xrip = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xrip) && IPAddress.TryParse(xrip, out var ip2))
            return (ip2.IsIPv4MappedToIPv6 ? ip2.MapToIPv4() : ip2).ToString();

        var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6Loopback;
        return (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString();
    }

    private static string ComputeFingerprint(string k, string s)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(k + s));
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return hex[..16];
    }
}
