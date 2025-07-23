using HNB.Models;
using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

public class HomeController : Controller
{
    private readonly HomeServices _homeServices;

    public HomeController(HomeServices homeServices)
        => _homeServices = homeServices;

    [Area("HNB_WEB")]
    public IActionResult Index()
    {
        _homeServices.PopulateHomeViewBag(ViewBag);
        return View();
    }

}
