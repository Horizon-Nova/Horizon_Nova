using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.DialVision.Controllers
{
    public class HomeController : Controller
    {
        [Area("DialVision")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
