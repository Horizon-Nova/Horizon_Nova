using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HNB.Areas.Weather.Services;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的登入頁面控制器，負責處理登入相關的頁面顯示功能
/// </summary>
[Area("Weather")]
public class LoginController(LoginService loginService) : Controller
{
    /// <summary>
    /// 顯示登入頁面
    /// </summary>
    /// <returns>返回登入頁面視圖</returns>
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Profile", new { area = "Weather" });
        }

        ViewBag.ReturnUrl = returnUrl ?? "/Weather/Profile/";
        return View();
    }

    /// <summary>
    /// 處理登入表單提交
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LoginFunction(string username, string password, bool rememberMe, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.ErrorMessage = "請輸入帳號和密碼";
            return View("Index");
        }

        var loginResult = loginService.ProcessLogin(username, password);
        
        if (!loginResult.success)
        {
            ViewBag.ErrorMessage = loginResult.errorMessage;
            return View("Index");
        }

        var user = loginResult.user!;
        
        // 創建 Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id?.ToString() ?? "0"),
            new Claim(ClaimTypes.Name, user.name ?? username),
            new Claim(ClaimTypes.Email, user.email ?? username),
            new Claim("FullName", user.full_name ?? user.name ?? ""),
            new Claim("OrganizationName", user.organization_name ?? "Whatever the wheather")
        };

        if (user.organization_id.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.organization_id.Value.ToString()));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties).GetAwaiter().GetResult();

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
            ? Redirect(returnUrl) 
            : RedirectToAction("Index", "Profile", new { area = "Weather" });
    }

    /// <summary>
    /// 處理登出
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
        return RedirectToAction("Index", "Login", new { area = "Weather" });
    }
}

