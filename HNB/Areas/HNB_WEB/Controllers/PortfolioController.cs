using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 作品集頁面
/// </summary>
[Area("HNB_WEB")]
public class PortfolioController(TeamZoneService service) : Controller
{
    /// <summary>
    /// 作品集頁面
    /// </summary>
    public IActionResult Index()
    {
        service.ViewBagPortfolioModel(ViewBag);
        return View();
    }
}

