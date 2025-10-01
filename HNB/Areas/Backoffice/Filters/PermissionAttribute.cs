using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HNB.Areas.Backoffice.Filters;

/// <summary>
/// 權限驗證屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string? _permission;
    private readonly string? _role;

    /// <summary>
    /// 基本權限驗證（僅檢查是否已登入）
    /// </summary>
    public PermissionAttribute(){}

    /// <summary>
    /// 權限驗證屬性
    /// </summary>
    /// <param name="permission">需要的權限</param>
    public PermissionAttribute(string permission)
    {
        _permission = permission;
    }

    /// <summary>
    /// 角色驗證屬性
    /// </summary>
    /// <param name="role">需要的角色</param>
    /// <param name="isRole">標記為角色驗證</param>
    public PermissionAttribute(string role, bool isRole = true)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 檢查是否有 UserName（帳號）
        var userName = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
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

        // 檢查角色（如果指定了角色）
        if (!string.IsNullOrEmpty(_role))
        {
            if (!context.HttpContext.User.IsInRole(_role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Authorize", 
                    new { area = "Backoffice" });
                return;
            }
        }
    }
}
