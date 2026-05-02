using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 諮詢頁面
/// </summary>
[Area("HNB_WEB")]
public class ConsultationController : Controller
{
    /// <summary>
    /// 諮詢頁面
    /// </summary>
    public IActionResult Index() => View();
}

