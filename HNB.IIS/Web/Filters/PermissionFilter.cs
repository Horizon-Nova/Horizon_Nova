using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class PermissionFilter(string requiredUser) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userName = context.HttpContext.User.Identity?.Name ?? "Anonymous";
        
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

