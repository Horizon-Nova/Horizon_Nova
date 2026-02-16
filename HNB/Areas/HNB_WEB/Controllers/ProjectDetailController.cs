using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 專案詳情頁面
/// </summary>
[Area("HNB_WEB")]
public class ProjectDetailController(TeamZoneService service) : Controller
{
    /// <summary>
    /// 專案詳情頁面
    /// </summary>
    /// <param name="id">專案 ID</param>
    public IActionResult Index(int? id = null)
    {
        var project = service.LoadProject(id);
        if (project == null)
            return RedirectToAction("Index", "Portfolio", new { area = "HNB_WEB" });

        return View(project);
    }
}

