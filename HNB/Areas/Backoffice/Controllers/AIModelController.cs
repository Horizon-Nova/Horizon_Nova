using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Services;
using System.Text.Json;

namespace HNB.Areas.Backoffice.Controllers
{
    [Area("Backoffice")]
    public class AIModelController : BaseController
    {
        public IActionResult AIModelManagement()
            => View();
    }
}
