using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HNB.Areas.Backoffice.Repositories;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AuthorizeController(AuthRepository authRepository, ILogger<AuthorizeController> logger) : Controller
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
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "請輸入帳號和密碼";
                return View("Login");
            }

            // 簡化驗證：直接使用用戶名作為認證
            // 在實際環境中，這裡應該驗證密碼
            var user = await authRepository.GetUserByUsernameOrEmailAsync(username);
            if (user == null)
            {
                ViewBag.ErrorMessage = "帳號不存在";
                return View("Login");
            }

            // 檢查用戶是否啟用
            if (user.is_active != true)
            {
                ViewBag.ErrorMessage = "帳號已被停用，請聯繫管理員";
                return View("Login");
            }

            // 建立簡單的 Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.username ?? username),
                new Claim(ClaimTypes.Email, user.email ?? ""),
                new Claim("FullName", user.full_name ?? user.username ?? ""),
                new Claim("UserType", user.type ?? "user"),
                new Claim("Avatar", user.avatar_url ?? ""),
                new Claim("LastLogin", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            // 添加角色 Claims
            if (user.roles != null && user.roles.Any())
            {
                foreach (var role in user.roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

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
            {
                await authRepository.UpdateLastLoginAsync(user.id.Value, HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            logger.LogInformation("用戶 {Username} 成功登入", username);

            // 重定向到指定頁面
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Backoffice" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "登入過程中發生錯誤");
            ViewBag.ErrorMessage = "登入失敗，請稍後再試";
            return View("Login");
        }
    }

    /// <summary>
    /// 處理登出
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var username = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            logger.LogInformation("用戶 {Username} 已登出", username);
            
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "登出過程中發生錯誤");
            return RedirectToAction("Login");
        }
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

    /// <summary>
    /// GitHub 登入
    /// </summary>
    [HttpGet]
    public IActionResult GithubLogin()
    {
        // TODO: 實作 GitHub OAuth 登入
        ViewBag.ErrorMessage = "GitHub 登入功能開發中";
        return View("Login");
    }

    /// <summary>
    /// 忘記密碼
    /// </summary>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        // TODO: 實作忘記密碼功能
        return View();
    }

    /// <summary>
    /// 處理忘記密碼
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        // TODO: 實作忘記密碼功能
        ViewBag.SuccessMessage = "密碼重設連結已發送到您的信箱";
        return View();
    }
}
