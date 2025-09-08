using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.HnbBackoffice.Services;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class AuthorizeController(AuthorizeService svc) : Controller
{
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginFuntion(string username, string password, string? returnUrl, CancellationToken ct)
    {
        var success = await svc.LoginAsync(username, password, ct);
        if (!success)
        {
            ViewBag.ErrorMessage = "帳號或密碼錯誤";
            return View("Login");
        }

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Dashboard", "Backoffice", new { area = "HnbBackoffice" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        svc.Logout();
        return RedirectToAction(nameof(Login));
    }
}
