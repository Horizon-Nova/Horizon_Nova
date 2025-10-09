using HNB.Areas.Backoffice.Repositories;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class SidebarNavigationController(SidebarNavigationService sev) : BaseController
{
    /// <summary>
    /// 目錄管理頁面（新版UI）
    /// </summary>
    public IActionResult NavigationManagement()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }


    /// <summary>
    /// 載入導航詳情（Partial View）
    /// </summary>
    public IActionResult LoadDetail(int id)
    {
        var results = sev.LoadNavigationById(id);
        return PartialView("_NavigationModal", results);
    }

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 新增導航項目
    /// </summary>
    [HttpPost]
    public IActionResult Create(sidebar_navigation form)
    {
        var result = sev.CreateNavigation(form);
        return Json(new { success = result != null, message = result != null ? "導航項目儲存成功" : "儲存失敗" });
    }

    #endregion

}
