using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

/// <summary>
/// 測試控制器 - 集中管理所有測試功能
/// </summary>
[Area("Backoffice")]
public class TestController : BaseController
{
    /// <summary>
    /// 測試功能首頁 - 顯示所有可用的測試項目
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Modal 彈出視窗測試頁面
    /// </summary>
    public IActionResult ModalTest()
    {
        return View();
    }

    /// <summary>
    /// 表單驗證測試頁面
    /// </summary>
    public IActionResult FormTest()
    {
        return View();
    }

    /// <summary>
    /// API 測試頁面
    /// </summary>
    public IActionResult ApiTest()
    {
        return View();
    }

    /// <summary>
    /// 元件測試頁面
    /// </summary>
    public IActionResult ComponentTest()
    {
        return View();
    }
}

