using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers
{
    [Area("HNB_WEB")]
    public class BackofficeController : Controller
    {
        public IActionResult BMS() => View();
        public IActionResult Login() => View();
        public IActionResult Dashboard() => View();
        public IActionResult ApiSecurity() => View();
        public IActionResult KeyManagement() => View();
        public IActionResult UserManagement() => View();
        #region Settings Page
        public IActionResult Settings() => View();
        public IActionResult Directory() => View();
        #endregion
        public IActionResult NotFound() => View();
    }
}
