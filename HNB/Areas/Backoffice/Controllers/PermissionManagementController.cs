using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, AuthService authService) : BaseController
{
    /// <summary>
    /// 獲取當前用戶的組織ID（從 Claims）
    /// </summary>
    private int? GetCurrentUserOrganizationId()
    {
        var organizationIdClaim = User.FindFirst("OrganizationId")?.Value;
        return int.TryParse(organizationIdClaim, out var orgId) ? orgId : null;
    }

    public IActionResult Users()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadUserList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    public IActionResult Roles()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadRoleList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    public IActionResult Organizations()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadOrganizationList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    public IActionResult OrganizationChart()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        var model = sev.LoadOrganizationList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    public IActionResult LoadDetail(int? id = null, string? type = null)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, id, currentUserOrganizationId);
        
        // 根據類型返回對應的 Modal Partial View
        return type switch
        {
            "user" => PartialView("_UsersModal"),
            "role" => PartialView("_RolesModal"),
            "organization" => PartialView("_OrganizationsModal"),
            "chart" => PartialView("_OrganizationChartModal"),
            _ => PartialView("_UsersModal") // 預設返回 Users Modal
        };
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = sev.Delete(id);
        return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
    }

    [HttpPost]
    public IActionResult SubmitUser(permission_management form, int[] role_ids) 
    {
        form.type = "user";
        form.roles = role_ids?.Select(id => id.ToString()).ToList() ?? [];
        
        if (!string.IsNullOrEmpty(form.password_hash))
        {
            var (hash, salt) = authService.HashNewPassword(form.password_hash);
            form.password_hash = hash;
            form.salt = salt;
        }
        
        var result = sev.CreateUser(form);
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    public IActionResult SubmitRole(permission_management form, string[] navigation_permissions) 
    {
        form.type = "role";
        form.navigation_permissions = navigation_permissions?.ToList() ?? [];
        
        var result = sev.CreateRole(form);
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    public IActionResult SubmitOrganization(permission_management form, int[] assigned_roles) 
    {
        form.type = "organization";
        form.roles = assigned_roles?.Select(id => id.ToString()).ToList() ?? [];
        
        var result = sev.CreateOrganization(form);
        return Json(new { success = result.success, message = result.message });
    }


}
