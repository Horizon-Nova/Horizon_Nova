using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Services;
using System.Text.Json;
using HNB.Areas.Backoffice.Filters;

namespace HNB.Areas.Backoffice.Controllers
{
    [Area("Backoffice")]
    public class AIModelController(SidebarNavigationService sidebarService) : BaseController(sidebarService)
    {
        public IActionResult AIModelManagement()
        {
            SetActiveNavigation("/Backoffice/AIModel/AIModelManagement");
            return View();
        }
    }
}
