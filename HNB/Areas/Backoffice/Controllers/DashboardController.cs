using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DashboardController : BaseController
{
    /// <summary>
    /// 後台儀表板首頁
    /// </summary>
    public IActionResult Dashboard()
    {
        return View();
    }
}
