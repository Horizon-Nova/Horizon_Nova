using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Services;

namespace Web.Controllers;

[RequireMing]
public class SiteManagementController(SiteManagementService service) : Controller
{
    public IActionResult Index()
    {
        ViewBag.Sites = service.LoadSiteList();
        return View();
    }

    [HttpPost]
    public IActionResult Stop(string siteName)
    {
        var result = service.StopSite(siteName);
        return Json(new { success = result, message = result ? "站台已停止" : "停止失敗" });
    }

    [HttpPost]
    public IActionResult Delete(string siteName)
    {
        var result = service.DeleteSite(siteName);
        return Json(new { success = result, message = result ? "站台已刪除" : "刪除失敗" });
    }

    [HttpGet]
    public IActionResult Status(string siteName)
    {
        var status = service.GetSiteStatus(siteName);
        return Json(new { status });
    }
}

