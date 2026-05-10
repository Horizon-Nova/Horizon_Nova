using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Vaeron.Controllers;

[Area("Vaeron")]
public class LandingController : Controller
{
    /// <summary>使用應用程式根目錄絕對路徑，避免部分環境下列不到 Area 視圖。</summary>
    public IActionResult Index() => View("~/Areas/Vaeron/Views/Landing/Index.cshtml");

    [HttpGet]
    [Route("/Vaeron/Consultation")]
    [Route("/Vaeron/Landing/Consultation")]
    public IActionResult Consultation() => View("~/Areas/Vaeron/Views/Landing/Consultation.cshtml");

    [HttpGet]
    [Route("/Vaeron/product")]
    [Route("/Vaeron/Landing/Product")]
    public IActionResult Product() => View("~/Areas/Vaeron/Views/Landing/Product.cshtml");

    [HttpGet]
    [Route("/Vaeron/product/{id?}")]
    [Route("/Vaeron/Landing/ProductDetail/{id?}")]
    public IActionResult ProductDetail(string? id) => View("~/Areas/Vaeron/Views/Landing/ProductDetail.cshtml");
}
