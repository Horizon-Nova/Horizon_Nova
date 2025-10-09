using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HNB.Areas.Backoffice.Services;
using System.Security.Claims;

namespace HNB.Areas.Backoffice.Filters;

/// <summary>
/// 權限驗證屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter
{

    /// <summary>
    /// 基本權限驗證（檢查登入狀態和角色導航權限）
    /// </summary>
    public PermissionAttribute(){}

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 檢查是否有當前登入用戶
        var currentUserName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUserName))
        {
            context.Result = new RedirectToActionResult("Login", "Authorize", 
                new { area = "Backoffice", returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // 檢查帳號是否正確（已登入）
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Authorize", 
                new { area = "Backoffice", returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // 暫時禁用權限檢查，僅檢查登入狀態
        // TODO: 調試完成後重新啟用權限檢查
        /*
        var currentPath = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (!await CheckUserNavigationPermissionAsync(context, userName, currentPath))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Authorize", 
                new { area = "Backoffice" });
            return;
        }
        */
    }


    /// <summary>
    /// 檢查用戶是否有當前 URL 的導航權限
    /// </summary>
    private Task<bool> CheckUserNavigationPermissionAsync(AuthorizationFilterContext context, string userName, string currentPath)
    {
        // 從 DI 容器獲取 SidebarNavigationService
        var sidebarService = context.HttpContext.RequestServices.GetService<SidebarNavigationService>();
        if (sidebarService == null) return Task.FromResult(false);

        // 獲取用戶的導航權限（會自動查詢用戶的所有角色權限）
        var userNavigations = sidebarService.LoadUserNavigationList(userName);
        
        // 如果用戶沒有任何導航權限，拒絕訪問
        if (!userNavigations.Any()) return Task.FromResult(false);
        
        // 檢查當前路徑是否在用戶的導航權限中
        // 使用更寬鬆的匹配邏輯：檢查路徑是否包含導航項目的 URL
        return Task.FromResult(userNavigations.Any(nav => 
            !string.IsNullOrEmpty(nav.url) && 
            (currentPath.StartsWith(nav.url.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase) ||
             currentPath.Contains(nav.url.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))));
    }
}
