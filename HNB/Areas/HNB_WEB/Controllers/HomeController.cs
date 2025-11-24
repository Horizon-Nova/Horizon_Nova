using System.Diagnostics;
using HNB.Areas.HNB_WEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 首頁控制器
/// </summary>
[Area("HNB_WEB")]
public class HomeController : Controller
{
    /// <summary>
    /// 首頁
    /// </summary>
    [HttpGet]
    public IActionResult Index()
        => View();

    /// <summary>
    /// 隱私權頁面
    /// </summary>
    [HttpGet]
    public IActionResult Privacy()
        => View();

    /// <summary>
    /// 錯誤頁面
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet]
    public IActionResult Error()
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View(model);
    }
}

