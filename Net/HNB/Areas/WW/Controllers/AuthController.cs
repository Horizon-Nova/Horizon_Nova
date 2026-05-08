using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class AuthController : Controller
    {
        #region Public Methods

        public IActionResult Index()
        {
            ViewData["Title"] = "WW Auth";
            return View();
        }

        #endregion
    }
}

