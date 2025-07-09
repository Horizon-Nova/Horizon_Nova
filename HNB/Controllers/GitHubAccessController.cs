using HNB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers;

public class GitHubAccessController : Controller
{
    private readonly GitHubAccessServices _services;
    private readonly IConfiguration _config;
    private readonly string _clientId, _clientSecret, _orgName, _callbackUrl;

    public GitHubAccessController(GitHubAccessServices services, IConfiguration config)
    {
        _services = services;
        _config = config;

        _clientId = config["GitHubOAuth:ClientId"];
        _clientSecret = config["GitHubOAuth:ClientSecret"];
        _orgName = config["GitHubOAuth:OrgName"];
        _callbackUrl = config["GitHubOAuth:CallbackUrl"];
    }

    // 登入畫面
    public IActionResult HNBAccess() => View();

    // 取登入連結
    [IgnoreAntiforgeryToken]
    public IActionResult RedirectGitHub()
    {
        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("OAuthState", state);

        var authorizeUrl =
            $"https://github.com/login/oauth/authorize" +
            $"?client_id={_clientId}" +
            $"&scope=read:user%20read:org" +
            $"&redirect_uri={_callbackUrl}" +
            $"&state={state}";

        return Redirect(authorizeUrl);
    }



    [HttpGet("github-access/callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        var expectedState = HttpContext.Session.GetString("OAuthState");
        HttpContext.Session.Remove("OAuthState");

        if (string.IsNullOrEmpty(expectedState) || expectedState != state)
        {
            TempData["Error"] = "無效的驗證請求，請重試。";
            return RedirectToAction("HNBAccess");
        }

        var results = await _services.TrySignInFromCodeAsync(code);

        if (results)
            return RedirectToAction("Team_introduction", "Overview");

        TempData["Error"] = "登入失敗，請確認您是否為 Horizon Nova 組織成員。";
        return RedirectToAction("HNBAccess");
    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _services.SignOutAsync();
        return RedirectToAction("Team_introduction", "Overview");
    }

}
