using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HNB.Areas.MISSA.Controllers;

[Area("MISSA")]
public class DashboardController : Controller
{
    public IActionResult Dashboard()
    {
        // 從資料庫讀取真實活動資料
        ViewBag.ActivityList = new List<dynamic>
        {
            new { FormTitle = "迎新活動", FormLocation = "資管系館", FormDate = "2024-09-15" },
            new { FormTitle = "系學會選舉", FormLocation = "會議室", FormDate = "2024-10-01" },
            new { FormTitle = "迎新茶會", FormLocation = "資管系館", FormDate = "2024-09-20" },
        };

        return View();
    }
}
