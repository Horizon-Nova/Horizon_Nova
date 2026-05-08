using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class LandingController : Controller
    {
        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "WW Landing";
            return View();
        }

        #endregion
    }
}

