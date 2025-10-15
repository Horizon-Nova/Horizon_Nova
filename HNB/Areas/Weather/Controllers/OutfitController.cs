using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的服裝搭配控制器，負責處理服裝搭配相關的頁面顯示功能
/// </summary>
[Area("Weather")]
public class OutfitController : Controller
{
    /// <summary>
    /// 顯示服裝搭配首頁
    /// </summary>
    /// <returns>返回首頁視圖</returns>
    public IActionResult Index()
        => View();
}
