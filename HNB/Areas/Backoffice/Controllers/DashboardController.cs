using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DashboardController(SidebarNavigationService sidebarService) : BaseController(sidebarService)
{
    /// <summary>
    /// 後台儀表板首頁
    /// </summary>
    public IActionResult Dashboard()
    {
        // 設置當前頁面導航狀態
        SetActiveNavigation("/Backoffice/Dashboard/Dashboard");
        
        return View();
    }
}
