using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Eos.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
