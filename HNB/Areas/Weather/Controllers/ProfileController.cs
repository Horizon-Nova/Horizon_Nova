using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HNB.Areas.Weather.Services;
using HNB.Areas.Weather.Filters;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的個人頁面控制器，負責處理個人資料相關的頁面顯示功能
/// </summary>
[Area("Weather")]
[WeatherAuth]
public class ProfileController(ProfileService profileService) : Controller
{
    /// <summary>
    /// 顯示個人頁面
    /// </summary>
    /// <returns>返回個人頁面視圖</returns>
    public IActionResult Index()
    {
        // 獲取當前登入用戶 ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Index", "Login", new { area = "Weather" });
        }

        // 獲取用戶資訊
        var user = profileService.GetUserProfile(userId);
        if (user == null)
        {
            return RedirectToAction("Index", "Login", new { area = "Weather" });
        }

        // 將用戶資訊傳遞給 View
        ViewBag.User = user;
        ViewBag.Year = DateTime.Now.Year;
        ViewBag.Month = DateTime.Now.Month;
        ViewBag.SelectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        return View();
    }
}

