using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class BackofficeController : Controller
{
    public IActionResult Dashboard()
        => View();

    public IActionResult Login()
        => View();
}
