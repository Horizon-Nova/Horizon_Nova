using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的申明頁面控制器，負責處理申明相關的頁面顯示功能
/// </summary>
[Area("Weather")]
public class DeclarationController : Controller
{
    /// <summary>
    /// 顯示申明頁面
    /// </summary>
    /// <returns>返回申明頁面視圖</returns>
    public IActionResult Index()
    {
        return View();
    }
}

