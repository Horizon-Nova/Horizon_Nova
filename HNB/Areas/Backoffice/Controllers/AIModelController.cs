using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Services;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AIModelController(AIModelServices svc) : BaseController
{
    public IActionResult AIModelManagement()
    {
        svc.ViewBagModel(ViewBag);
        return View();
    }

    [HttpGet]
    public IActionResult LoadDetail(long? id = null)
    {
        svc.ViewBagModel(ViewBag, id);
        return PartialView("_AIModelModal");
    }

    [HttpPost]
    public IActionResult SubmitConfig(ai_config form)
    {
        var result = svc.CreateAIConfig(form);
        return Json(new { success = result != null, message = result != null ? "儲存成功" : "儲存失敗" });
    }

    [HttpPost]
    public IActionResult Delete(long id)
    {
        var result = svc.DeleteAIConfig(id);
        return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
    }
}
