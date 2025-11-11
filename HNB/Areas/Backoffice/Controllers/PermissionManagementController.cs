using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, AuthService authService) : BaseController
{
    #region 共用工具
    /// <summary>
    /// 獲取當前用戶的組織ID（從 Claims）
    /// </summary>
    private int? GetCurrentUserOrganizationId()
    {
        var organizationIdClaim = User.FindFirst("OrganizationId")?.Value;
        return int.TryParse(organizationIdClaim, out var orgId) ? orgId : null;
    }
	#endregion

    #region 使用者管理
    public IActionResult Users()
        => View();

    public IActionResult SearchUsers()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();

        var model = sev.LoadUserList(organizationId: currentUserOrganizationId);
        return PartialView("Partials/Users/_SearchResults", model);
    }

    [HttpGet]
    public IActionResult LoadUserDetail(int id)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.User as vw_permission_user;
        return PartialView("Partials/Users/Modal/_Permissions", model);
    }

    [HttpGet]
    public IActionResult LoadUserForm(int? id = null)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.User as vw_permission_user;
        return PartialView("Partials/Users/Modal/_FormData", model);
    }

    #endregion

    #region 角色管理
    public IActionResult Roles()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadRoleList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    [HttpGet]
    public IActionResult SearchRoles()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();

        var model = sev.LoadRoleList(organizationId: currentUserOrganizationId);
        return PartialView("Partials/Roles/_SearchResults", model);
    }

    [HttpGet]
    public IActionResult LoadRoleDetail(int id)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.Role as vw_permission_role;
        return PartialView("Partials/Roles/Modal/_Permissions", model);
    }

    [HttpGet]
    public IActionResult LoadRoleForm(int? id = null)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.Role as vw_permission_role;
        return PartialView("Partials/Roles/Modal/_FormData", model);
    }

	#endregion

    #region 組織管理
    public IActionResult Organizations()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        
        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadOrganizationList(organizationId: currentUserOrganizationId);
        return View(model);
    }

    public IActionResult SearchOrganizations()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();

        var model = sev.LoadOrganizationList(organizationId: currentUserOrganizationId);
        return PartialView("Partials/Organizations/_SearchResults", model);
    }

    public IActionResult LoadOrganizationDetail(int id)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.Organization as vw_permission_organization;
        return PartialView("Partials/Organizations/Modal/_Detail", model);
    }

    public IActionResult LoadOrganizationForm(int? id = null)
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();
        sev.ViewBagModel(ViewBag, id: id, currentUserOrganizationId: currentUserOrganizationId);
        var model = ViewBag.Organization as vw_permission_organization;
        return PartialView("Partials/Organizations/Modal/_FormData", model);
    }

    public IActionResult OrganizationChart()
    {
        var currentUserOrganizationId = GetCurrentUserOrganizationId();

        sev.ViewBagModel(ViewBag, currentUserOrganizationId: currentUserOrganizationId);
        
        var model = sev.LoadOrganizationList(organizationId: currentUserOrganizationId);
        return View(model);
    }
	#endregion

	#region 基本 CRUD 操作

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
    public IActionResult SubmitOrganization(permission_management form) 
    {
        form.type = "organization";
        form.roles = new List<string>(); // 不再分配角色給組織
        
        var result = sev.CreateOrganization(form);
        return Json(new { success = result.success, message = result.message });
    }
	#endregion

}
