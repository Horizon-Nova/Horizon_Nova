using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Services;
using System.Text.Json;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class AIModelController(AIModelServices svc) : BaseController
{
    public IActionResult AIModelManagement()
    {
        svc.ViewBagModel(ViewBag);
        return View();
    }

    #region 統一的查詢方法
    #endregion

    #region 基本 CRUD 操作

    #endregion
}
