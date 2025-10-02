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
        return View(users);
    }

    // 角色管理
    public IActionResult Roles()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/Roles");
        var roles = sev.LoadRoles();
        return View(roles);
    }

    // 組織管理
    public IActionResult Organizations()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/Organizations");
        var organizations = sev.LoadOrganizations();
        return View(organizations);
    }

    // 組織圖
    public IActionResult OrganizationChart()
    {
        SetActiveNavigation("/Backoffice/PermissionManagement/OrganizationChart");
        var organizations = sev.LoadOrganizations();
        return View(organizations);
    }

    // 獲取用戶詳細資訊 (AJAX)
    public IActionResult LoadUserDetails(int id)
    {
        try
        {
            var user = sev.LoadUserDetails(id);
            if (user == null)
            {
                ViewBag.Error = "找不到指定的使用者";
                return PartialView("_UserDetailContent");
            }
            
            ViewBag.User = user;
            return PartialView("_UserDetailContent");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return PartialView("_UserDetailContent");
        }
    }

    // 獲取角色詳細資訊 (AJAX)
    public IActionResult LoadRoleDetails(int id)
    {
        try
        {
            var role = sev.LoadRoleDetails(id);
            if (role == null)
            {
                ViewBag.Error = "找不到指定的角色";
                return PartialView("_RoleDetailContent");
            }
            
            ViewBag.Role = role;
            return PartialView("_RoleDetailContent");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return PartialView("_RoleDetailContent");
        }
    }

    // 獲取組織詳細資訊 (AJAX)
    public IActionResult LoadOrganizationDetails(int id)
    {
        try
        {
            var organization = sev.LoadOrganizationDetails(id);
            if (organization == null)
            {
                ViewBag.Error = "找不到指定的組織";
                return PartialView("_OrganizationDetailContent");
            }
            
            ViewBag.Organization = organization;
            return PartialView("_OrganizationDetailContent");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return PartialView("_OrganizationDetailContent");
        }
    }

    // 刪除使用者 (AJAX)
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await sev.DeleteUserAsync(id);
            return Json(new { success = result, message = result ? "使用者刪除成功" : "刪除失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 刪除角色 (AJAX)
    [HttpPost]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var result = await sev.DeleteRoleAsync(id);
            return Json(new { success = result, message = result ? "角色刪除成功" : "刪除失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 刪除組織 (AJAX)
    [HttpPost]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        try
        {
            var result = await sev.DeleteOrganizationAsync(id);
            return Json(new { success = result, message = result ? "組織刪除成功" : "刪除失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 統一載入表單 (AJAX)
    public IActionResult LoadForm(string type, string formAction, int? id = null)
    {
        try
        {
            ViewBag.Type = type;
            ViewBag.Action = formAction;
            
            if (formAction?.Trim() == "edit" && id.HasValue)
            {
                // 編輯模式 - 載入現有資料
                switch (type)
                {
                    case "user":
                        var user = sev.LoadUserDetails(id.Value);
                        if (user != null)
                        {
                            // 設定角色的預設值 - 從 roles 陣列中取得第一個角色ID
                            var roleId = user.roles?.FirstOrDefault();
                            if (!string.IsNullOrEmpty(roleId) && int.TryParse(roleId, out var parsedRoleId))
                            {
                                ViewBag.SelectedRoleId = parsedRoleId;
                            }
                            
                            // 獲取使用者的組織ID
                            ViewBag.SelectedOrganizationId = sev.GetUserOrganizationId(id.Value);
                        }
                        return PartialView("_UserForm", user);
                    case "role":
                        var role = sev.LoadRoleDetails(id.Value);
                        return PartialView("_RoleForm", role);
                    case "organization":
                        var organization = sev.LoadOrganizationDetails(id.Value);
                        return PartialView("_OrganizationForm", organization);
                    default:
                        return Json(new { success = false, message = "不支援的類型" });
                }
            }
            else if (formAction?.Trim() == "add")
            {
                // 新增模式 - 建立空物件
                switch (type)
                {
                    case "user":
                        return PartialView("_UserForm", new vw_permission_user());
                    case "role":
                        return PartialView("_RoleForm", new vw_permission_role());
                    case "organization":
                        return PartialView("_OrganizationForm", new vw_permission_organization());
                    default:
                        return Json(new { success = false, message = "不支援的類型" });
                }
            }
            else
            {
                return Json(new { success = false, message = "不支援的操作" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 統一儲存資料 (AJAX)
    [HttpPost]
    public IActionResult SaveData(string type)
    {
        var form = Request.Form;
        var id = int.TryParse(form["id"], out var parsedId) ? parsedId : 0;
        var action = id > 0 ? "edit" : "add";
        
        switch (type)
        {
            case "user":
                return SaveUser(form, action);
            case "role":
                return SaveRole(form, action);
            case "organization":
                return SaveOrganization(form, action);
            default:
                return Json(new { success = false, message = "不支援的類型" });
        }
    }

    private IActionResult SaveUser(IFormCollection form, string action)
    {
        // Controller 只負責接收資料並傳給服務層
        var result = sev.SaveUser(form, action);
        return Json(new { success = result.success, message = result.message });
    }

    private IActionResult SaveRole(IFormCollection form, string action)
    {
        var result = sev.SaveRole(form, action);
        return Json(new { success = result.success, message = result.message });
    }

    private IActionResult SaveOrganization(IFormCollection form, string action)
    {
        var result = sev.SaveOrganization(form, action);
        return Json(new { success = result.success, message = result.message });
    }

    /// <summary>
    /// 載入角色和組織選項
    /// </summary>
    /// <returns>角色和組織選項的JSON</returns>
    [HttpGet]
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
    /// 載入說明視窗
    /// </summary>
    /// <param name="type">說明類型 (userform, roleform, organizationform, userdetail, organizationdetail)</param>
    /// <returns>說明視窗的PartialView</returns>
    [HttpGet]
    public IActionResult LoadHelp(string type)
    {
        return type?.ToLower() switch
        {
            "userform" => PartialView("_UserFormHelp"),
            "roleform" => PartialView("_RoleFormHelp"),
            "organizationform" => PartialView("_OrganizationFormHelp"),
            "userdetail" => PartialView("_UserDetailHelp"),
            "organizationdetail" => PartialView("_OrganizationDetailHelp"),
            _ => PartialView("_UserFormHelp") // 預設返回使用者表單說明
        };
    }

    /// <summary>
    /// 取得導航選項
    /// </summary>
    /// <returns>導航選項JSON</returns>
    [HttpGet]
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
    [HttpGet]
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
    [HttpGet]
    public IActionResult GetRoleNavigationPermissions(int roleId)
    {
        var permissions = sev.GetRoleNavigationPermissions(roleId);
        return Json(new { success = true, permissions });
    }
}
