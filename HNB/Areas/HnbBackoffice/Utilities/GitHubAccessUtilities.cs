namespace HNB.Areas.HnbBackoffice.Utilities;

public class GitHubAccessUtilities
{
    #region 產生授權網址（建立 state、保存 returnUrl）
    public string BuildAuthorizeUrl(HttpContext http, IConfiguration cfg, string? returnUrl)
    {
        var state = Guid.NewGuid().ToString("N");
        http.Session.SetString("OAuthState", state);

        if (!string.IsNullOrWhiteSpace(returnUrl))
            http.Session.SetString("OAuthReturnUrl", returnUrl);

        var cb = Uri.EscapeDataString(cfg["GitHubOAuth:CallbackUrl"] ?? "");
        var cid = Uri.EscapeDataString(cfg["GitHubOAuth:ClientId"] ?? "");
        const string scope = "read:user%20read:org";

        return "https://github.com/login/oauth/authorize" +
               $"?client_id={cid}&scope={scope}&redirect_uri={cb}&state={state}";
    }
    #endregion

    #region 驗證 state
    public (bool ok, string? err) ValidateState(HttpContext http, string? providedState)
    {
        var expected = http.Session.GetString("OAuthState");
        http.Session.Remove("OAuthState");
        if (string.IsNullOrEmpty(expected) || expected != providedState)
            return (false, "授權逾時或請求不合法（state）。");
        return (true, null);
    }
    #endregion

    #region popup 旗標存取
    public void SaveIsPopup(HttpContext http, bool isPopup)
        => http.Session.SetString("OAuthIsPopup", isPopup ? "1" : "0");

    public bool ReadAndClearIsPopup(HttpContext http)
    {
        var v = http.Session.GetString("OAuthIsPopup");
        http.Session.Remove("OAuthIsPopup");
        return v == "1";
    }
    #endregion

    #region 是否檢查 Org
    public bool ShouldCheckOrg(IConfiguration cfg)
        => !string.IsNullOrWhiteSpace(cfg["GitHubOAuth:OrgName"]);
    #endregion

    #region 安全挑選回跳路徑（僅允許相對本機路徑）
    public string PickRedirectPath(HttpContext http, string? queryReturnUrl, string fallbackPath)
    {
        string? stored = http.Session.GetString("OAuthReturnUrl");
        http.Session.Remove("OAuthReturnUrl");

        string? firstSafe(string? u)
        {
            if (string.IsNullOrWhiteSpace(u)) return null;
            if (u.StartsWith('/') && !u.StartsWith("//") && !u.Contains("://"))
                return u;
            return null;
        }

        return firstSafe(queryReturnUrl)
            ?? firstSafe(stored)
            ?? (string.IsNullOrWhiteSpace(fallbackPath) ? "/" : fallbackPath);
    }
    #endregion
}
