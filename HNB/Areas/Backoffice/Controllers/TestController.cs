using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class TestController(SidebarNavigationService sidebarService, PermissionManagementService permissionService) : BaseController(sidebarService)
{
    /// <summary>
    /// 測試頁面（不需要登入）
    /// </summary>
    public IActionResult Test()
    {
        SetActiveNavigation("/Backoffice/Test/Test");
        
        ViewBag.Message = "測試頁面 - 不需要登入";
        
        return View();
    }
    
    /// <summary>
    /// 測試登入狀態
    /// </summary>
    public IActionResult CheckAuth()
    {
        var result = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name ?? "未登入",
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
        
        return Json(result);
    }
    
    /// <summary>
    /// 調試導航權限
    /// </summary>
    public async Task<IActionResult> DebugNavigation()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { error = "未登入" });
        }

        var userName = User.Identity.Name ?? "";
        
        // 取得用戶導航
        var userNavigation = await _sidebarService.GetUserNavigationAsync(userName);
        
        // 取得所有導航
        var allNavigation = _sidebarService.GetAllNavigations();
        
        var result = new
        {
            UserName = userName,
            UserNavigationCount = userNavigation.Count,
            UserNavigation = userNavigation.Select(n => new {
                n.Code,
                n.Title,
                n.ParentCode,
                n.Url,
                n.SortOrder,
                n.IsActive,
                ChildrenCount = n.Children.Count,
                Children = n.Children.Select(c => new { c.Code, c.Title, c.IsActive }).ToList()
            }).ToList(),
            AllNavigationCount = allNavigation.Count,
            AllNavigation = allNavigation.Select(n => new {
                n.Code,
                n.Title,
                n.ParentCode,
                n.Url,
                n.SortOrder,
                n.IsActive,
                ChildrenCount = n.Children.Count
            }).ToList()
        };
        
        return Json(result);
    }
    
    /// <summary>
    /// 測試角色資料
    /// </summary>
    public IActionResult TestRoles()
    {
        try
        {
            var roles = permissionService.LoadRoles();
            var result = new
            {
                Success = true,
                Count = roles.Count,
                Roles = roles.Select(r => new
                {
                    r.id,
                    r.name,
                    r.description,
                    r.is_active,
                    r.organization_name,
                }).ToList()
            };
            
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { Success = false, Error = ex.Message });
        }
    }
}
