using Microsoft.AspNetCore.Mvc;

namespace HNB.Controllers;

public class ErrorController : Controller
{
    public IActionResult NotFound()
        => View();
}
