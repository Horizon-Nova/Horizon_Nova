using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
[Permission] // 所有繼承此BaseController的頁面都需要權限驗證
public abstract class BaseController(SidebarNavigationService sidebarService) : Controller
{
    protected readonly SidebarNavigationService _sidebarService = sidebarService;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 載入側欄導航數據
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
            var userName = User.Identity.Name ?? "";
            var navigationItems = await _sidebarService.GetUserNavigationAsync(userName);
            ViewBag.SidebarNavigation = navigationItems;
        }
        else
        {
            ViewBag.SidebarNavigation = new List<NavigationItem>();
        }
    }

    /// <summary>
    /// 設定當前活躍的導航項目 URL
    /// </summary>
    protected void SetActiveNavigation(string url)
    {
        ViewBag.ActiveNavigationUrl = url.ToLowerInvariant();
    }
}
