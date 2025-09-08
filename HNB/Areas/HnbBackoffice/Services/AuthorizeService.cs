using HNB.Utilities;
using static System.Net.WebRequestMethods;

namespace HNB.Areas.HnbBackoffice.Services;

public class AuthorizeService(DbKeyJwtService DBsvc, IHttpContextAccessor http)
{

    public async Task<bool> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var ok = (username == "admin" && password == "123456");
        if (!ok) return false;

        var httpCtx = http.HttpContext!;
        var (token, _exp) = await DBsvc.IssueTokenAfterLoginAsync(
            ctx: httpCtx,
            keyComponents: $"login_{username}",
            note: "登入產生",
            ct: ct);

        CookieUtil.SetAuthCookie(httpCtx, token, crossSite: false);

        return true;
    }

    /// <summary>
    /// 登出：僅刪除 Cookie（不產任何新 Cookie）
    /// </summary>
    public void Logout()
    {
        var httpCtx = http.HttpContext!;
        CookieUtil.DeleteAuthCookie(httpCtx, crossSite: false);
    }
}
