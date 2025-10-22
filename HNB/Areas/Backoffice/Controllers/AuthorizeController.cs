using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Filters;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AuthorizeController(AuthService authService, PermissionManagementService permissionService) : Controller
{

    /// <summary>
    /// 顯示登入頁面
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
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
    public IActionResult LoginFuntion(string username, string password, bool rememberMe, string? returnUrl = null)
    {
        var loginResult = authService.ProcessLogin(username, password);
        
        if (!loginResult.success)
        {
            ViewBag.ErrorMessage = loginResult.errorMessage;
            return View("Login");
        }

        var user = loginResult.user!;
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id?.ToString() ?? "0"),
            new Claim(ClaimTypes.Name, user.name ?? username),
            new Claim(ClaimTypes.Email, user.email ?? ""),
            new Claim("FullName", user.full_name ?? user.name ?? ""),
            new Claim("UserType", user.type ?? "user"),
            new Claim("Avatar", user.avatar_url ?? ""),
            new Claim("LastLogin", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
        };

        // 添加組織資訊
        if (user.organization_id.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.organization_id.Value.ToString()));
        }
        if (!string.IsNullOrEmpty(user.organization_name))
        {
            claims.Add(new Claim("OrganizationName", user.organization_name));
        }

        // 查詢角色名稱（而不是只用ID）
        var roleNames = new List<string>();
        if (user.roles != null && user.roles.Any())
        {
            foreach (var roleId in user.roles)
            {
                if (int.TryParse(roleId, out var id))
                {
                    var role = permissionService.LoadRole(id);
                    if (role != null && !string.IsNullOrEmpty(role.name))
                    {
                        roleNames.Add(role.name);
                        claims.Add(new Claim(ClaimTypes.Role, role.name));  // 使用角色名稱
                    }
                }
            }
        }

        // 主要角色顯示（用於 Layout）
        var primaryRoleName = roleNames.FirstOrDefault() ?? "一般用戶";
        claims.Add(new Claim("Role", primaryRoleName));

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
            : RedirectToAction("Dashboard", "Dashboard", new { area = "Backoffice" });
    }

    /// <summary>
    /// 處理登出
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
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
