using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Eos.Controllers
{
    [Area("Eos")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
