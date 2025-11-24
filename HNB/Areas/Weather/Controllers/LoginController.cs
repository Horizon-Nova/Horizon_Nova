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
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Profile", new { area = "Weather" });
        }
        return View();
    }

    /// <summary>
    /// 處理登入表單提交（AJAX）
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Json(new { success = false, errorMessage = "請輸入帳號和密碼" });
        }

        var loginResult = loginService.ProcessLogin(request.Email, request.Password);
        
        if (!loginResult.success)
        {
            return Json(new { success = false, errorMessage = loginResult.errorMessage });
        }

        var user = loginResult.user!;
        
        // 創建 Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id?.ToString() ?? "0"),
            new Claim(ClaimTypes.Name, user.name ?? request.Email),
            new Claim(ClaimTypes.Email, user.email ?? request.Email),
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
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        return Json(new { success = true, redirectUrl = Url.Action("Index", "Profile", new { area = "Weather" }) });
    }

    /// <summary>
    /// 登入請求模型
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

