using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev) : BaseController
{

    // 人員管理
    public IActionResult Users()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    // 角色管理
    public IActionResult Roles()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    // 組織管理
    public IActionResult Organizations()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    // 組織圖
    public IActionResult OrganizationChart()
    {
        sev.ViewBagModel(ViewBag);
        return View();
    }

    // 載入統一的彈出視窗
    public IActionResult LoadDetail(int? id = null)
    {
        sev.ViewBagModel(ViewBag, id);
        return PartialView("_Detail");
    }

    // 統一刪除方法
    [HttpPost]
    public IActionResult Delete(int id) 
        => Json(new { success = sev.Delete(id), message = sev.Delete(id) ? "刪除成功" : "刪除失敗" });

    // 提交用戶資料
    [HttpPost]
    public IActionResult SubmitUser(permission_management form, int[] role_ids) 
    {
        // 設定用戶類型
        form.type = "user";
        
        // 確保 role_ids 被正確設定
        if (role_ids != null && role_ids.Length > 0)
        {
            form.roles = role_ids.Select(id => id.ToString()).ToList();
        }
        else
        {
            form.roles = new List<string>();
        }
        
        return Json(new { success = sev.CreateUser(form).success, message = sev.CreateUser(form).message });
    }

    // 提交角色資料
    [HttpPost]
    public IActionResult SubmitRole(permission_management form, string[] navigation_permissions) 
    {
        // 設定角色類型
        form.type = "role";
        
        // 確保 navigation_permissions 被正確設定
        if (navigation_permissions != null)
        {
            form.navigation_permissions = navigation_permissions.ToList();
        }
        else
        {
            form.navigation_permissions = new List<string>();
        }
        
        return Json(new { success = sev.CreateRole(form).success, message = sev.CreateRole(form).message });
    }

    // 提交組織資料
    [HttpPost]
    public IActionResult SubmitOrganization(permission_management form, int[] assigned_roles) 
    {
        // 設定組織類型
        form.type = "organization";
        
        // 確保 assigned_roles 被正確設定
        if (assigned_roles != null && assigned_roles.Length > 0)
        {
            form.roles = assigned_roles.Select(id => id.ToString()).ToList();
        }
        else
        {
            form.roles = new List<string>();
        }
        
        return Json(new { success = sev.CreateOrganization(form).success, message = sev.CreateOrganization(form).message });
    }


}
