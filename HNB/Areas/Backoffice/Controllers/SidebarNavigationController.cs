using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class SidebarNavigationController(SidebarNavigationService svc) : BaseController
{

    /// <summary>
    /// 側欄導航管理首頁
    /// </summary>
    public IActionResult SidebarNavigation()
    {
        svc.ViewBagModel(ViewBag);
        return View();
    }

}
