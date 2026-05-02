using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 首頁（NovaHome）
/// </summary>
[Area("HNB_WEB")]
public class NovaHomeController : Controller
{
    /// <summary>
    /// 首頁
    /// </summary>
    public IActionResult Index() => View();
}

