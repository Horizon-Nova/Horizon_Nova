using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNB.Filters;

public class OperationPermissionAttribute : TypeFilterAttribute
{
    public OperationPermissionAttribute() : base(typeof(OperationPermissionFilter)) { }
}

public class OperationPermissionFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("HNBAccess", "GitHubAccess", null);
        }
    }
}
