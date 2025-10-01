using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Services;
using System.Text.Json;
using HNB.Areas.Backoffice.Filters;

namespace HNB.Areas.Backoffice.Controllers
{
    [Area("Backoffice"), Permission]
    public class AIModelController : Controller
    {
        public IActionResult AIModelManagement()
            => View();
    }
}
