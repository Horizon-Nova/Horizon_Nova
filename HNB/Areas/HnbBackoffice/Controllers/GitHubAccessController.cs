using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class GitHubAccessController(GitHubAccessServices svc) : Controller
{
    #region 取得登入連結（支援 popup）
    [HttpGet("github-access/login")]
    [IgnoreAntiforgeryToken]
    public IActionResult RedirectGitHub(string? returnUrl = null, bool popup = false)
    {
        var authorizeUrl = svc.BuildAuthorizeUrl(HttpContext, returnUrl, popup);
        return Redirect(authorizeUrl);
    }
    #endregion

    #region GitHub 回呼（popup 用 postMessage，否則一般 Redirect）
    [HttpGet("github-access/callback")]
    public async Task<IActionResult> Callback(string code, string state, string? returnUrl, CancellationToken ct)
    {
        var result = await svc.HandleCallbackAsync(HttpContext, code, state, returnUrl,
            fallbackPath: "/HnbBackoffice/Backoffice/Dashboard", ct: ct);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "登入失敗";
            return Redirect("/HnbBackoffice/Authorize/Login");
        }

        if (result.IsPopup)
        {
            var html = $@"<!doctype html><meta charset=""utf-8""><script>
try {{
  if (window.opener) {{
    window.opener.postMessage({{ type:'GH_AUTH_DONE', redirectTo:'{result.RedirectTo}' }}, window.location.origin);
  }}
}} catch(e) {{}} finally {{
  window.close();
  setTimeout(function(){{ location.href = '{result.RedirectTo}'; }}, 300);
}}
</script>";
            return Content(html, "text/html; charset=utf-8");
        }

        return Redirect(result.RedirectTo);
    }
    #endregion
}
