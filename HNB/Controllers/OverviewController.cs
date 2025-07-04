using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers
{
    public class OverviewController : Controller
    {
        public IActionResult Team_introduction() => View();
    }
}
