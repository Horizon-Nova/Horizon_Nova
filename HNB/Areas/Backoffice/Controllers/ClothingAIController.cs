using Microsoft.AspNetCore.Mvc;
using HNB.Areas.Backoffice.Controllers;

namespace HNB.Areas.Backoffice.Controllers;

public class ClothingAIController : BaseController
{
    public IActionResult Wardrobe()
        => View();

    public IActionResult Assistant()
        => View();
}
