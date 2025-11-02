using HNB.IIS.Core.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HNB.IIS.Core.Controllers;

public class AccountController(PermissionRepository permissionRepo) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        // 從資料庫查詢用戶
        var user = permissionRepo.QueryUserByName(username);
        
        if (user != null && VerifyPassword(password, user.password_hash, user.salt))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "SiteManagement");
        }

        ViewData["Error"] = "帳號或密碼錯誤";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // 驗證密碼（使用 salt）
    private bool VerifyPassword(string password, string? storedHash, string? salt)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(salt))
            return false;

        var hash = HashPassword(password, salt);
        return hash == storedHash;
    }

    // 產生密碼雜湊（SHA256 + Salt）
    private string HashPassword(string password, string salt)
    {
        var saltedPassword = password + salt;
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(bytes);
    }
}

