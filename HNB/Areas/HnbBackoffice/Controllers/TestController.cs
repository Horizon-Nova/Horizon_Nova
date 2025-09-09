using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class TestController : Controller
{
    public IActionResult Index()
        => View();

}
