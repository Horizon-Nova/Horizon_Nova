using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Models.HnbBackoffice;
using Microsoft.Extensions.DependencyInjection;


namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice"),Permission]
public abstract class BaseController : Controller
{

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LoadSidebarNavigation();
        base.OnActionExecuting(context);
    }

    /// <summary>
    /// 載入側欄導航數據到 ViewBag（根據用戶角色權限）
    /// </summary>
    protected void LoadSidebarNavigation()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var sidebarService = HttpContext.RequestServices.GetRequiredService<SidebarNavigationService>();
            var currentUserName = User.Identity?.Name ?? string.Empty;
            ViewBag.SidebarNavigation = sidebarService.LoadUserNavigationList(currentUserName);
        }
        else
        {
            ViewBag.SidebarNavigation = new List<vw_sidebar_navigation>();
        }
    }

}
