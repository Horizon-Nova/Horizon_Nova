using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 產品頁面（原型：先做版面，不接真實資料）
/// </summary>
[Area("HNB_WEB")]
[Route("product")]
public class ProductController : Controller
{
    /// <summary>
    /// 產品列表頁
    /// </summary>
    [HttpGet("")]
    public IActionResult Index() => View();

    /// <summary>
    /// 產品介紹頁（以 slug 區分）
    /// </summary>
    [HttpGet("{slug}")]
    public IActionResult Detail(string slug)
    {
        ViewData["Slug"] = slug;
        return View();
    }
}

