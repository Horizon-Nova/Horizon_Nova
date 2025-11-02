using Microsoft.AspNetCore.Mvc;

namespace HNB.IIS.Core.Controllers;

public class ErrorController : Controller
{
    public new IActionResult NotFound() => View();
}

