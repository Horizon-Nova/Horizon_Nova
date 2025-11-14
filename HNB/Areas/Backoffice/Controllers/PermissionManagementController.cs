using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using HNB.Areas.Backoffice.Core;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, AuthService authService, OrganizationScope organizationScope) : BaseController
{
    #region 共用工具
    /// <summary>
    /// 獲取當前用戶的組織範圍 ID 列表
    /// </summary>
    private List<int> GetCurrentUserScopeIds()
    {
        var scope = organizationScope.ResolveUserScope(User);
        return scope.ScopeIds;
    }
    #endregion

    #region 使用者管理
    public IActionResult Users()
        => View();

    public IActionResult SearchUsers()
    {
        var scopeIds = GetCurrentUserScopeIds();
        var model = sev.LoadUserList(organizationIds: scopeIds);
        return PartialView("Partials/Users/_SearchResults", model);
    }

    public IActionResult LoadUserDetail(int id)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = sev.LoadUser(id);
        return PartialView("Partials/Users/Modal/_Permissions", model);
    }

    public IActionResult LoadUserForm(int? id = null)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = id.HasValue ? sev.LoadUser(id.Value) : null;
        return PartialView("Partials/Users/Modal/_FormData", model);
    }

    #endregion

    #region 角色管理
    public IActionResult Roles()
        => View();

    public IActionResult SearchRoles()
    {
        var scopeIds = GetCurrentUserScopeIds();
        var model = sev.LoadRoleList(organizationIds: scopeIds);
        return PartialView("Partials/Roles/_SearchResults", model);
    }

    public IActionResult LoadRoleDetail(int id)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = sev.LoadRole(id);
        return PartialView("Partials/Roles/Modal/_Permissions", model);
    }

    public IActionResult LoadRoleForm(int? id = null)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = id.HasValue ? sev.LoadRole(id.Value) : null;
        return PartialView("Partials/Roles/Modal/_FormData", model);
    }

    #endregion

    #region 組織管理
    public IActionResult Organizations()
        => View();

    public IActionResult SearchOrganizations()
    {
        var scopeIds = GetCurrentUserScopeIds();
        var model = sev.LoadOrganizationList(organizationIds: scopeIds);
        return PartialView("Partials/Organizations/_SearchResults", model);
    }

    public IActionResult LoadOrganizationDetail(int id)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = sev.LoadOrganization(id);
        return PartialView("Partials/Organizations/Modal/_Detail", model);
    }

    public IActionResult LoadOrganizationForm(int? id = null)
    {
        var scopeIds = GetCurrentUserScopeIds();
        sev.ViewBagModel(ViewBag, organizationIds: scopeIds);
        var model = id.HasValue ? sev.LoadOrganization(id.Value) : null;
        return PartialView("Partials/Organizations/Modal/_FormData", model);
    }

    #endregion

    #region 組織圖
    public IActionResult OrganizationChart()
        => View();

    public IActionResult LoadOrganizationChart()
    {
        var scopeIds = GetCurrentUserScopeIds();
        var model = sev.LoadOrganizationList(organizationIds: scopeIds);
        return Json(model);
    }
    #endregion

    #region 基本 CRUD 操作

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var result = sev.DeletePermissionManagement(id);
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
