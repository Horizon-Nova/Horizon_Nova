using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的服裝搭配控制器，負責處理服裝搭配相關的頁面顯示功能
/// </summary>
[Area("Weather")]
public class EntryController : Controller
{
    /// <summary>
    /// 顯示入口功能頁面（Try it for Free 進入）
    /// </summary>
    /// <returns>返回入口功能視圖</returns>
    public IActionResult Index()
        => View();
}
