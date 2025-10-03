using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class PermissionManagementController(PermissionManagementService sev, SidebarNavigationService sidebarService) : BaseController(sidebarService)
{

    // 人員管理
    public IActionResult Users()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/Users");
        var users = sev.LoadUsers();
        sev.ViewBagModelUsers(ViewBag);
        return View(users);
    }

    // 角色管理
    public IActionResult Roles()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/Roles");
        var roles = sev.LoadRoles();
        sev.ViewBagModelRoles(ViewBag);
        return View(roles);
    }

    // 組織管理
    public IActionResult Organizations()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/Organizations");
        var organizations = sev.LoadOrganizations();
        sev.ViewBagModelOrganizations(ViewBag);
        return View(organizations);
    }

    // 組織圖
    public IActionResult OrganizationChart()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/OrganizationChart");
        var organizations = sev.LoadOrganizations();
        return View(organizations);
    }

    // 載入統一的彈出視窗
    public IActionResult LoadDetail(string type, int? id = null)
    {
        sev.ViewBagModelPM(ViewBag, type, id);
        return PartialView("_Detail");
    }


    // 統一刪除方法
    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            var result = sev.Delete(id);
            return Json(new { success = result, message = result ? "刪除成功" : "刪除失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    // 統一儲存資料
    [HttpPost]
    public IActionResult SaveData(string type)
    {
        var form = Request.Form;
        
        switch (type)
        {
            case "user":
                return SaveUser(form);
            case "role":
                return SaveRole(form);
            case "organization":
                return SaveOrganization(form);
            default:
                return Json(new { success = false, message = "不支援的類型" });
        }
    }

    private IActionResult SaveUser(IFormCollection form)
    {
        // Controller 只負責接收資料並傳給服務層
        var result = sev.SaveUser(form);
        return Json(new { success = result.success, message = result.message });
    }

    private IActionResult SaveRole(IFormCollection form)
    {
        var result = sev.SaveRole(form);
        return Json(new { success = result.success, message = result.message });
    }

    private IActionResult SaveOrganization(IFormCollection form)
    {
        var result = sev.SaveOrganization(form);
        return Json(new { success = result.success, message = result.message });
    }

    /// <summary>
    /// 載入角色和組織選項
    /// </summary>
    /// <returns>角色和組織選項的JSON</returns>
    
    public IActionResult LoadUserOptions()
    {
        try
        {
            var roles = sev.LoadRoles();
            var organizations = sev.LoadOrganizations();
            
            return Json(new { 
                success = true, 
                roles = roles.Select(r => new { id = r.id, name = r.name }),
                organizations = organizations.Select(o => new { id = o.id, name = o.name })
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    /// <summary>
    /// 取得導航選項
    /// </summary>
    /// <returns>導航選項JSON</returns>
    
    public IActionResult GetNavigationOptions()
    {
        var navigations = sidebarService.GetAllNavigations();
        
        var navigationOptions = navigations.Select(nav => new
        {
            code = nav.Code,
            title = nav.Title,
            icon = nav.Icon,
            url = nav.Url
        }).ToList();

        return Json(new { success = true, navigations = navigationOptions });
    }

    /// <summary>
    /// 取得可用角色（未分配給其他組織的角色）
    /// </summary>
    /// <param name="organizationId">組織ID（編輯時使用）</param>
    /// <returns>可用角色JSON</returns>
    
    public IActionResult GetAvailableRoles(int organizationId = 0)
    {
        var availableRoles = sev.GetAvailableRoles(organizationId);
        
        var roleOptions = availableRoles.Select(role => new
        {
            id = role.id,
            name = role.name,
            description = role.description
        }).ToList();

        return Json(new { success = true, roles = roleOptions });
    }

    /// <summary>
    /// 取得角色的導航權限
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>導航權限JSON</returns>
    
    public IActionResult GetRoleNavigationPermissions(int roleId)
    {
        var permissions = sev.GetRoleNavigationPermissions(roleId);
        return Json(new { success = true, permissions });
    }
}
