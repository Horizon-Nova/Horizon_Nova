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
    /// 載入導航資料（通用：詳情、編輯、刪除都用這個）
    /// </summary>
    public IActionResult LoadDetail(int id)
    {
        var result = sev.LoadNavigationById(id);
        return PartialView("_NavigationModal", result);
    }

    /// <summary>
    /// 載入上層目錄選項
    /// </summary>
    public IActionResult LoadParentOptions()
    {
        var navigations = sev.LoadAllNavigations();
        var options = navigations
            .Where(n => string.IsNullOrEmpty(n.parent_code)) 
            .Select(n => new
            {
                code = n.code,
                title = n.title,
                full_path = n.full_path
            })
            .ToList();
        
        return Json(options);
    }

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 提交導航項目（新增或編輯）
    /// </summary>
    [HttpPost]
    public IActionResult SubmitNavigation(sidebar_navigation form)
    {
        var result = sev.CreateNavigation(form);
        return Json(new { success = result != null, message = result != null ? "導航項目新增成功" : "新增失敗" });
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = sev.DeleteNavigation(id);
        return Json(new { success = result, message = result ? "導航項目刪除成功" : "刪除失敗，可能包含子項目" });
    }

    #endregion
}
