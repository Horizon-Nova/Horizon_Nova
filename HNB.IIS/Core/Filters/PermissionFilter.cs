using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNB.IIS.Core.Filters;

public class PermissionFilter(string requiredUser) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // 檢查是否已登入
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            // 未登入，重定向到登入頁面
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }
        
        // 已登入，檢查是否為指定用戶
        var userName = user.Identity?.Name ?? "Anonymous";
        if (userName != requiredUser)
        {
            context.Result = new RedirectToActionResult("NotFound", "Error", null);
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireMingAttribute : TypeFilterAttribute
{
    public RequireMingAttribute() : base(typeof(PermissionFilter))
    {
        Arguments = new object[] { "Ming" };
    }
}

