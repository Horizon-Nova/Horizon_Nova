using HNB.Areas.Backoffice.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
[Permission] // 僅檢查登入狀態
public class DashboardController : Controller
{
    /// <summary>
    /// 後台儀表板首頁
    /// </summary>
    public IActionResult Dashboard()
    {
        // 獲取當前用戶資訊
        var userName = User.Identity?.Name ?? "未知用戶";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var fullName = User.FindFirst("FullName")?.Value ?? "";
        var userType = User.FindFirst("UserType")?.Value ?? "";
        var lastLogin = User.FindFirst("LastLogin")?.Value ?? "";

        ViewBag.UserName = userName;
        ViewBag.UserEmail = userEmail;
        ViewBag.FullName = fullName;
        ViewBag.UserType = userType;
        ViewBag.LastLogin = lastLogin;

        return View();
    }
}
