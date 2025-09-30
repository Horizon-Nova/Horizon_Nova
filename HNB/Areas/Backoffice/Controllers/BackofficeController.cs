using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class BackofficeController : Controller
{
    public IActionResult Dashboard()
        => View();
}
