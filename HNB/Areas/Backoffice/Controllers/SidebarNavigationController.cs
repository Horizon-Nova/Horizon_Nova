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
        return PartialView("_NavigationManagementModal", result);
    }

    /// <summary>
    /// 載入上層目錄選項（以 PartialView 回傳 <option>）
    /// </summary>
    public IActionResult LoadParentOptions()
    {
        var navigations = sev.LoadAllNavigations()
            .OrderBy(n => n.full_path)
            .ToList();
        return PartialView("_ParentOptions", navigations);
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

    /// <summary>
    /// 批量更新排序順序
    /// </summary>
    [HttpPost]
    public IActionResult UpdateSortOrder([FromBody] List<SortOrderUpdate> updates)
    {
        if (updates == null || !updates.Any())
        {
            return Json(new { success = false, message = "更新資料不能為空" });
        }

        foreach (var update in updates)
        {
            // 查詢完整物件
            var nav = sev.LoadNavigationById(update.id);
            if (nav != null)
            {
                // 只更新 sort_order
                var fullNav = new sidebar_navigation
                {
                    id = nav.id ?? 0,
                    code = nav.code ?? "",
                    title = nav.title ?? "",
                    url = nav.url ?? "",
                    icon = nav.icon,
                    sort_order = update.sort_order,
                    parent_code = nav.parent_code,
                    is_active = nav.is_active
                };
                sev.CreateNavigation(fullNav);
            }
        }
        
        return Json(new { success = true, message = "排序更新成功" });
    }

    #endregion
}

/// <summary>
/// 排序更新資料模型（僅包含必要欄位）
/// </summary>
public class SortOrderUpdate
{
    public int id { get; set; }
    public int sort_order { get; set; }
}
