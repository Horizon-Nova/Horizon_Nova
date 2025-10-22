using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Models.HnbHnbBackoffice;


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
            var sidebarService = HttpContext.RequestServices.GetService<SidebarNavigationService>();
            if (sidebarService != null)
            {
                var currentUserName = User.Identity.Name ?? "";
                var navigationList = sidebarService.LoadUserNavigationList(currentUserName);
                ViewBag.SidebarNavigation = navigationList;
            }
            else
            {
                ViewBag.SidebarNavigation = new List<vw_sidebar_navigation>();
            }
        }
        else
        {
            ViewBag.SidebarNavigation = new List<vw_sidebar_navigation>();
        }
    }

}
