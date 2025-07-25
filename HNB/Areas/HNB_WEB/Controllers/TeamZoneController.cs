using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

namespace HNB.Areas.HNB_WEB.Controllers
{
    [Area("HNB_WEB")]
    public class TeamZoneController : Controller
    {
        private readonly TeamZoneServices _teamZoneServices;

        public TeamZoneController(TeamZoneServices teamZoneServices)
        {
            _teamZoneServices = teamZoneServices;
        }

        public IActionResult Index()
        {
            _teamZoneServices.PopulateTeamZoneViewBag(ViewBag);
            return View();
        }

        public IActionResult Skin()
            => View();

        public IActionResult Welcome()
            => View();
    }
}
