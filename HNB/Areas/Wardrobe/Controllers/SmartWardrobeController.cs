// using 部分視情況調整
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HNB.Areas.Wardrobe.Controllers;

[Area("Wardrobe")]
public class SmartWardrobeController : Controller
{
    [HttpGet]
    public IActionResult WardrobeTool()
    {
        return View();
    }

}
