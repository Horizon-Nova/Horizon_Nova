using HNB.Models;
using HNB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers;

public class HomeController : Controller
{
    private readonly HomeServices _homeServices;

    public HomeController(HomeServices homeServices)
        => _homeServices = homeServices;

    public IActionResult Index()
    {
        _homeServices.PopulateHomeViewBag(ViewBag);
        return View();
    }

}
