using HNB.Areas.WebTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WebTest.Controllers;

[Area("WebTest")]
public class WebTestController : Controller
{

    [AcceptVerbs("GET", "POST")]
    [IgnoreAntiforgeryToken]
    public IActionResult FormTest([FromForm] string keyword)
        => HttpMethods.IsGet(Request.Method) ? Json("Running Get") : Json("Running Post");
}
