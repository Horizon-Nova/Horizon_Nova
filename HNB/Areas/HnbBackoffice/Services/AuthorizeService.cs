using HNB.Utilities;

namespace HNB.Areas.HnbBackoffice.Services;

public class AuthorizeService(DbKeyJwtService DBsvc, IHttpContextAccessor http)
{
    /// <summary>
    /// 登入：驗證通過後由 DbKeyJwtService 產生 JWT 並在內部直接寫入 Cookie
    /// </summary>
    public bool Login(string username, string password)
    {
        var ok = (username == "admin" && password == "123456");
        if (!ok) return false;

        var httpCtx = http.HttpContext!;
        DBsvc.IssueTokenAfterLogin(ctx: httpCtx,keyComponents: $"login_{username}",note: "登入產生");

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
