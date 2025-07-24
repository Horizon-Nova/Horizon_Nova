using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.DialVision.Controllers
{
    public class AppWebController : Controller
    {
        [Area("DialVision")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
