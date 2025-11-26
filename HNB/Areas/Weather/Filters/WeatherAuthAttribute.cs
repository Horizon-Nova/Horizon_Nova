using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNB.Areas.Weather.Filters;

/// <summary>
/// Weather 區域認證屬性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class WeatherAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = context.HttpContext.Request.Path.Value;
            context.Result = new RedirectToActionResult("Index", "Login",
                new { area = "Weather", returnUrl = returnUrl });
        }
    }
}

