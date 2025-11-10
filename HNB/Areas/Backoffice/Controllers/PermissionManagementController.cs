using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, AuthService authService) : BaseController
{
	#region Utilities
    /// <summary>
    /// 獲取當前用戶的組織ID（從 Claims）
    /// </summary>
    private int? GetCurrentUserOrganizationId()
    {
        var organizationIdClaim = User.FindFirst("OrganizationId")?.Value;
        return int.TryParse(organizationIdClaim, out var orgId) ? orgId : null;
    }
	#endregion

	#region Users
    public IActionResult Users()
        => View();

    public IActionResult UserSearch()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();

        var model = sev.LoadUserList(organizationId: currentUserOrganizationId);
        return PartialView("Partials/Users/_UserResults", model);
    }

    #endregion

    #region Roles
    public IActionResult Roles()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadRoleList(organizationId: currentUserOrganizationId);
        return View(model);
    }
	#endregion

	#region Organizations
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
	#endregion

	#region Shared / Partials (AJAX)

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = sev.Delete(id);
        return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
    }

	#endregion

	#region Submit (Create/Update)
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
    public IActionResult SubmitOrganization(permission_management form) 
    {
        form.type = "organization";
        form.roles = new List<string>(); // 不再分配角色給組織
        
        var result = sev.CreateOrganization(form);
        return Json(new { success = result.success, message = result.message });
    }
	#endregion


}
