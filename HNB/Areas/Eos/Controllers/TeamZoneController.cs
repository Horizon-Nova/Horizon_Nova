using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Eos.Controllers
{
    [Area("Eos")]
    public class TeamZoneController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
