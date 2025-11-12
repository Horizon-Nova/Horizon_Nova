using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 團隊作品集頁面控制器。
/// </summary>
[Area("HNB_WEB")]
public class TeamZoneController(TeamZoneService service) : Controller
{
    private readonly TeamZoneService _service = service;

    [HttpGet]
    public IActionResult NovaHome()
        => View();

    [HttpGet]
    public IActionResult Consultation()
        => View();

    [HttpGet]
    public IActionResult ProjectDetail(int id)
    {
        var project = _service.LoadProject(id);
        if (project is null)
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(project);
    }

    [HttpGet]
    public IActionResult Portfolio()
    {
        _service.ViewBagPortfolioModel(ViewBag);
        return View();
    }
}

