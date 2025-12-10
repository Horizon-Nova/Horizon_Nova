using HNB.Areas.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的測試控制器，用於測試頁面編輯功能
/// </summary>
[Area("Weather")]
public class TestController(PageEditorService pageEditorService) : Controller
{
    /// <summary>
    /// 顯示測試頁面
    /// </summary>
    /// <returns>返回測試視圖</returns>
    public IActionResult Index()
        => View();

    /// <summary>
    /// 顯示頁面編輯介面
    /// </summary>
    /// <returns>返回頁面編輯視圖</returns>
    public IActionResult PageEditor()
    {
        var sections = pageEditorService.GetEditableSections("Test");
        var sectionsWithDisplayName = sections.Select(s => new
        {
            name = s,
            displayName = pageEditorService.GetSectionDisplayName(s)
        }).ToList();
        ViewBag.Sections = sectionsWithDisplayName;
        return View();
    }
}

