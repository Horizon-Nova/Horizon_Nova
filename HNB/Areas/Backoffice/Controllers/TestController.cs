using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 測試控制器 - 集中管理所有測試功能
/// </summary>
[Area("Backoffice")]
public class TestController : Controller
{
    /// <summary>
    /// 測試功能首頁 - 顯示所有可用的測試項目
    /// </summary>
    public IActionResult Index()
        => View();

}

