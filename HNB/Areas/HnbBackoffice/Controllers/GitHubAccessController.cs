using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class GitHubAccessController(GitHubAccessServices svc) : Controller
{
    #region 取得授權網址
    [IgnoreAntiforgeryToken]
    public IActionResult GetAuthorizeUrl(string? returnUrl = null)
    {
        var url = svc.BuildAuthorizeUrl(HttpContext, returnUrl, popup: false);
        return Json(new { ok = true, url });
    }
    #endregion

    #region 取得登入連結（相容舊邏輯：立即 302 導向）
    [IgnoreAntiforgeryToken]
    public IActionResult RedirectGitHub(string? returnUrl = null)
    {
        var authorizeUrl = svc.BuildAuthorizeUrl(HttpContext, returnUrl, popup: false);
        return Redirect(authorizeUrl);
    }
    #endregion

    #region GitHub 回呼
    public async Task<IActionResult> Callback(string code, string state, string? returnUrl, CancellationToken ct)
    {
        var result = await svc.HandleCallbackAsync(
            HttpContext, code, state, returnUrl,
            fallbackPath: "/HnbBackoffice/Backoffice/Dashboard", ct: ct);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "登入失敗";
            return Redirect("/HnbBackoffice/Authorize/Login");
        }
        return Redirect(result.RedirectTo);
    }
    #endregion
}

