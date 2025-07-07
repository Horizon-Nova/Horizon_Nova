using HNB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers;

public class GitHubAccessController : Controller
{
    private readonly GitHubAccessServices _services;
    private readonly IConfiguration _config;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _orgName;

    public GitHubAccessController(GitHubAccessServices services, IConfiguration config)
    {
        _services = services;
        _config = config;

        _clientId = config["GitHubOAuth:ClientId"];
        _clientSecret = config["GitHubOAuth:ClientSecret"];
        _orgName = config["GitHubOAuth:OrgName"];
    }

    public IActionResult HNBAccess() => View();

    public IActionResult RedirectGitHub()
    {
        var clientId = _config["GitHubOAuth:ClientId"];
        var callbackUrl = _config["GitHubOAuth:CallbackUrl"];
        var authorizeUrl =
            $"https://github.com/login/oauth/authorize?client_id={clientId}&scope=read:user%20read:org&redirect_uri={callbackUrl}";
        return Redirect(authorizeUrl);
    }

    [HttpGet("github-access/callback")]
    public async Task<IActionResult> Callback(string code)
    {
        var ok = await _services.TrySignInFromCodeAsync(code);

        if (ok)
            return RedirectToAction("Team_introduction", "Overview");

        TempData["Error"] = "登入失敗，請確認您是否為 Horizon Nova 組織成員。";
        return RedirectToAction("HNBAccess");
    }
}
