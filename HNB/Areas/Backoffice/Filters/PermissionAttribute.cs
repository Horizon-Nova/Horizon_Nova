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
    public PermissionAttribute() { }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUserName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUserName))
        {
            context.Result = new RedirectToActionResult("Login", "Authorize",
                new { area = "Backoffice", returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Authorize",
                new { area = "Backoffice", returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // 檢查導航權限
        var currentPath = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (!await CheckUserNavigationPermissionAsync(context, currentUserName, currentPath))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Authorize",
                new { area = "Backoffice" });
            return;
        }
    }


    /// <summary>
    /// 檢查用戶是否有當前 URL 的導航權限
    /// </summary>
    private Task<bool> CheckUserNavigationPermissionAsync(AuthorizationFilterContext context, string userName, string currentPath)
    {
        var whitelistPaths = new[]
        {
            "/backoffice/dashboard",
            "/backoffice/backoffice/profile",
            "/backoffice/backoffice/changepassword",
            "/backoffice/authorize"
        };
        
        if (whitelistPaths.Any(path => currentPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(true);
        }
        
        var sidebarService = context.HttpContext.RequestServices.GetService<SidebarNavigationService>();
        if (sidebarService == null) return Task.FromResult(false);

        var userNavigations = sidebarService.LoadUserNavigationList(userName);
        
        if (!userNavigations.Any()) return Task.FromResult(false);
        
        var pathSegments = currentPath.TrimStart('/').Split('/');
        if (pathSegments.Length >= 2)
        {
            var currentController = $"/{pathSegments[0]}/{pathSegments[1]}";
            
            return Task.FromResult(userNavigations.Any(nav => 
                !string.IsNullOrEmpty(nav.url) && 
                nav.url.ToLowerInvariant().StartsWith(currentController, StringComparison.OrdinalIgnoreCase)));
        }
        
        return Task.FromResult(userNavigations.Any(nav => 
            !string.IsNullOrEmpty(nav.url) && 
            (currentPath.StartsWith(nav.url.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase) ||
             currentPath.Contains(nav.url.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))));
    }
}
