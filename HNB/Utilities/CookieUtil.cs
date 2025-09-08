using Microsoft.AspNetCore.Http;

namespace HNB.Utilities;

public static class CookieUtil
{
    public const string AuthCookieName = "HNB_API_TOKEN";

    public static void SetAuthCookie(HttpContext ctx, string token, bool crossSite = false)
    {
        var opt = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Path = "/",
        };

        opt.SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Strict;

        ctx.Response.Cookies.Append(AuthCookieName, token, opt);
    }

    public static void DeleteAuthCookie(HttpContext ctx, bool crossSite = false)
    {
        var opt = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Path = "/",
            SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Strict
        };
        ctx.Response.Cookies.Delete(AuthCookieName, opt);
    }
}
