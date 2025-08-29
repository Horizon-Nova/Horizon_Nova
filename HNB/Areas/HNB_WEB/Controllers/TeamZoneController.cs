using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

namespace HNB.Areas.HNB_WEB.Controllers;

[Area("HNB_WEB")]
public class TeamZoneController(TeamZoneServices sev) : Controller
{
    public IActionResult NovaHome()
        => View();
    public IActionResult Consultation()
        => View();

    public IActionResult ProjectDetail(int id)
    {
        var model = sev.ProjectDetailData(id);
        return View(model);
    }

    public IActionResult Portfolio()
    {
        sev.ViewBagModelPortfolio(ViewBag);
        return View();
    }

    public IActionResult NotFound()
        => View();

}
