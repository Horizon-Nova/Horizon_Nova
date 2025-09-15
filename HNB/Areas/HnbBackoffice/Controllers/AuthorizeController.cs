using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class AuthorizeController(AuthorizeService svc) : Controller
{
    // 允許匿名進入登入頁
    [HttpGet,AllowAnonymous]
    public IActionResult Login() => View();

    // 允許匿名提交登入
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
    public IActionResult LoginFuntion(string username, string password, string? returnUrl)
    {
        var success = svc.Login(username, password);
        if (!success)
        {
            ViewBag.ErrorMessage = "帳號或密碼錯誤";
            return View("Login");
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Dashboard", "Backoffice", new { area = "HnbBackoffice" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        svc.Logout();
        return RedirectToAction(nameof(Login));
    }
}
