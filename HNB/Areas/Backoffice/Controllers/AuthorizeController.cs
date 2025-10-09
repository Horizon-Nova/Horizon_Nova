using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Filters;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AuthorizeController(AuthService authService) : Controller
{

    /// <summary>
    /// 顯示登入頁面
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // 如果已經登入，重定向到後台首頁
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Dashboard", new { area = "Backoffice" });
        }

        ViewBag.ReturnUrl = returnUrl ?? "/Backoffice/";
        return View();
    }

    /// <summary>
    /// 處理登入表單提交
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginFuntion(string username, string password, bool rememberMe, string? returnUrl = null)
    {
        var loginResult = await authService.ProcessLoginAsync(username, password);
        
        if (!loginResult.success)
        {
            ViewBag.ErrorMessage = loginResult.errorMessage;
            return View("Login");
        }

        var user = loginResult.user!;
        
        // 建立 Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
            new Claim(ClaimTypes.Name, user.name ?? username),
            new Claim(ClaimTypes.Email, user.email ?? ""),
            new Claim("FullName", user.full_name ?? user.name ?? ""),
            new Claim("UserType", user.type ?? "user"),
            new Claim("Avatar", user.avatar_url ?? ""),
            new Claim("LastLogin", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
        };

        // 添加角色 Claims
        user.roles?.ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        // 登入用戶
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), authProperties);

        // 更新最後登入時間
        if (user.id.HasValue)
            await authService.UpdateLastLoginAsync(user.id.Value, HttpContext.Connection.RemoteIpAddress?.ToString());

        // 重定向到指定頁面
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
            ? Redirect(returnUrl) 
            : RedirectToAction("Dashboard", "Dashboard", new { area = "Backoffice" });
    }

    /// <summary>
    /// 處理登出
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    /// <summary>
    /// 存取被拒絕頁面
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "存取被拒絕";
        return View();
    }

}
