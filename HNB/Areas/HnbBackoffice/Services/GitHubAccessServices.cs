using System.Net.Http.Headers;
using System.Text.Json;
using HNB.Areas.HnbBackoffice.Utilities;

namespace HNB.Areas.HnbBackoffice.Services;

public record GitHubCallbackResult(bool Success, string? Error, string RedirectTo, bool IsPopup);

public class GitHubAccessServices(IHttpClientFactory httpFactory, IConfiguration cfg, GitHubAccessUtilities util, DbKeyJwtService jwtSvc)
{
    #region 建立授權網址（建立 state/returnUrl 與 popup 旗標）
    public string BuildAuthorizeUrl(HttpContext http, string? returnUrl, bool popup)
    {
        util.SaveIsPopup(http, popup);
        return util.BuildAuthorizeUrl(http, cfg, returnUrl);
    }
    #endregion

    #region 回呼流程
    public async Task<GitHubCallbackResult> HandleCallbackAsync(
        HttpContext http, string code, string state, string? queryReturnUrl, string fallbackPath, CancellationToken ct)
    {
        var (okState, err) = util.ValidateState(http, state);
        var isPopup = util.ReadAndClearIsPopup(http);
        if (!okState) return new(false, err, "/HnbBackoffice/Authorize/Login", isPopup);

        var accessToken = await ExchangeTokenAsync(code, ct);
        if (string.IsNullOrWhiteSpace(accessToken))
            return new(false, "無法取得 GitHub access_token。", "/HnbBackoffice/Authorize/Login", isPopup);

        var user = await GetUserAsync(accessToken, ct);
        if (user is null)
            return new(false, "無法讀取 GitHub 使用者資訊。", "/HnbBackoffice/Authorize/Login", isPopup);

        if (util.ShouldCheckOrg(cfg))
        {
            var okMember = await IsOrgMemberAsync(accessToken, user.Value.Login, ct);
            if (!okMember)
                return new(false, "您非指定 GitHub 組織成員。", "/HnbBackoffice/Authorize/Login", isPopup);
        }

        var (token, exp) = await jwtSvc.IssueTokenAfterLoginAsync(
            http, $"github_{user.Value.Login}", "GitHub 登入", ct);

        http.Response.Cookies.Append("HNB_API_TOKEN", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = exp.UtcDateTime
        });

        var redirectTo = util.PickRedirectPath(http, queryReturnUrl, fallbackPath);
        return new(true, null, redirectTo, isPopup);
    }
    #endregion

    #region GitHub API：交換 token
    private async Task<string?> ExchangeTokenAsync(string code, CancellationToken ct)
    {
        var client = httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = cfg["GitHubOAuth:ClientId"] ?? "",
                ["client_secret"] = cfg["GitHubOAuth:ClientSecret"] ?? "",
                ["code"] = code
            })
        };
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        return doc.RootElement.TryGetProperty("access_token", out var token) ? token.GetString() : null;
    }
    #endregion

    #region GitHub API：取得使用者
    private async Task<(string Login, string Name)?> GetUserAsync(string accessToken, CancellationToken ct)
    {
        var client = httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Headers.UserAgent.ParseAdd("HorizonNovaApp/1.0");

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return null;

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var login = doc.RootElement.GetProperty("login").GetString();
        var name = doc.RootElement.TryGetProperty("name", out var n) ? (n.GetString() ?? login) : login;
        return (login!, name!);
    }
    #endregion

    #region GitHub API：檢查 Org 成員
    private async Task<bool> IsOrgMemberAsync(string accessToken, string login, CancellationToken ct)
    {
        var org = cfg["GitHubOAuth:OrgName"] ?? "";
        if (string.IsNullOrWhiteSpace(org)) return true;

        var client = httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/orgs/{org}/memberships/{login}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Headers.UserAgent.ParseAdd("HorizonNovaApp/1.0");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode) return false;

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        return doc.RootElement.TryGetProperty("state", out var stateProp)
            && string.Equals(stateProp.GetString(), "active", StringComparison.OrdinalIgnoreCase);
    }
    #endregion
}
