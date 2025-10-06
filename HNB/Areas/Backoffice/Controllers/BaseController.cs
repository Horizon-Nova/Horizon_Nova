using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice"),Permission]
public abstract class BaseController : Controller
{

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await LoadSidebarNavigationAsync();
        
        await base.OnActionExecutionAsync(context, next);
    }

    /// <summary>
    /// 載入側欄導航數據到 ViewBag
    /// </summary>
    protected async Task LoadSidebarNavigationAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var sidebarService = HttpContext.RequestServices.GetService<SidebarNavigationService>();
            if (sidebarService != null)
            {
                var userName = User.Identity.Name ?? "";
                var navigationItems = await sidebarService.GetUserNavigationAsync(userName);
                ViewBag.SidebarNavigation = navigationItems;
            }
            else
            {
                ViewBag.SidebarNavigation = new List<NavigationItem>();
            }
        }
        else
        {
            ViewBag.SidebarNavigation = new List<NavigationItem>();
        }
    }

}
