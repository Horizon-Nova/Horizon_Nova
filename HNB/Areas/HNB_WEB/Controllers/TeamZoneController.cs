using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 團隊作品集頁面控制器
/// </summary>
[Area("HNB_WEB")]
public class TeamZoneController(TeamZoneService service) : Controller
{
    /// <summary>
    /// 首頁
    /// </summary>
    [HttpGet]
    public IActionResult NovaHome()
        => View();

    /// <summary>
    /// 諮詢頁面
    /// </summary>
    [HttpGet]
    public IActionResult Consultation()
        => View();

    /// <summary>
    /// 專案詳情頁面
    /// </summary>
    /// <param name="id">專案 ID</param>
    [HttpGet]
    public IActionResult ProjectDetail(int? id = null)
    {
        var project = service.LoadProject(id);
        if (project == null)
            return RedirectToAction(nameof(Portfolio));

        return View(project);
    }

    /// <summary>
    /// 作品集頁面
    /// </summary>
    [HttpGet]
    public IActionResult Portfolio()
    {
        service.ViewBagPortfolioModel(ViewBag);
        return View();
    }
}

