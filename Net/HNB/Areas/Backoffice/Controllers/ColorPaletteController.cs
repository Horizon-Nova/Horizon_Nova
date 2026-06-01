using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class ColorPaletteController : BaseController
{
    /// <summary>
    /// 顏色表主頁面
    /// </summary>
    public IActionResult Index()
        => View();
}