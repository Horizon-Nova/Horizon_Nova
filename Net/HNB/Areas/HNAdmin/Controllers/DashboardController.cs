using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNAdmin.Controllers;

[Area("HNAdmin")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
