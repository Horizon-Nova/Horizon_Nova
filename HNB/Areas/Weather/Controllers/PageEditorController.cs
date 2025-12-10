using HNB.Areas.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Weather.Controllers;

/// <summary>
/// Weather 區域的頁面編輯控制器，負責處理頁面編輯功能
/// </summary>
[Area("Weather")]
public class PageEditorController(PageEditorService pageEditorService) : Controller
{
    /// <summary>
    /// 顯示頁面編輯主頁面
    /// </summary>
    /// <returns>返回編輯頁面視圖</returns>
    public IActionResult Index()
    {
        var pages = pageEditorService.GetEditablePages();
        ViewBag.Pages = pages;
        return View();
    }

    /// <summary>
    /// 載入編輯 Modal
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <returns>返回編輯 Modal Partial View</returns>
    public IActionResult LoadDetail(string? pageName = null, string? sectionName = null)
    {
        ViewBag.PageName = pageName ?? "";
        ViewBag.SectionName = sectionName ?? "";
        
        if (!string.IsNullOrEmpty(pageName) && !string.IsNullOrEmpty(sectionName))
        {
            var content = pageEditorService.LoadFileContent(pageName, sectionName);
            ViewBag.Content = content ?? "";
        }
        else
        {
            ViewBag.Content = "";
        }
        
        return PartialView("Partials/_EditModal");
    }

    /// <summary>
    /// 儲存編輯內容
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <param name="content">檔案內容</param>
    /// <returns>返回 JSON 結果</returns>
    [HttpPost]
    public IActionResult SubmitEdit(string pageName, string sectionName, string content)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(sectionName))
        {
            return Json(new { success = false, message = "頁面名稱或區塊名稱不能為空" });
        }

        var result = pageEditorService.SaveFileContent(pageName, sectionName, content);
        
        if (result)
        {
            return Json(new { success = true, message = "儲存成功" });
        }
        else
        {
            return Json(new { success = false, message = "儲存失敗" });
        }
    }

    /// <summary>
    /// 取得指定頁面的區塊列表
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <returns>返回 JSON 結果</returns>
    [HttpGet]
    public IActionResult GetSections(string pageName)
    {
        if (string.IsNullOrEmpty(pageName))
        {
            return Json(new { success = false, sections = new List<object>() });
        }

        var sections = pageEditorService.GetEditableSections(pageName);
        var sectionsWithDisplayName = sections.Select(s => new
        {
            name = s,
            displayName = pageEditorService.GetSectionDisplayName(s)
        }).ToList();

        return Json(new { success = true, sections = sectionsWithDisplayName });
    }

    /// <summary>
    /// 載入 Partial 預覽內容
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <returns>返回 Partial View</returns>
    [HttpGet]
    public IActionResult LoadPartialPreview(string pageName, string sectionName)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(sectionName))
        {
            return Content("<div class='text-danger text-center p-4'>參數錯誤</div>");
        }

        ViewBag.PageName = pageName;
        ViewBag.SectionName = sectionName;
        ViewBag.EditorMode = true; // 標記為編輯模式，會加上可編輯標記
        
        // 如果是 Styles，直接返回
        if (sectionName == "Styles")
        {
            return PartialView($"~/Areas/Weather/Views/{pageName}/Partials/_{sectionName}.cshtml");
        }
        
        // 其他 Partial 需要同時載入 Styles
        ViewBag.IncludeStyles = true;
        return PartialView($"~/Areas/Weather/Views/{pageName}/Partials/_{sectionName}.cshtml");
    }
    
    /// <summary>
    /// 取得元素編輯器
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <param name="elementId">元素 ID</param>
    /// <returns>返回元素編輯器 Partial View</returns>
    [HttpGet]
    public IActionResult LoadElementEditor(string pageName, string sectionName, string elementId)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(sectionName) || string.IsNullOrEmpty(elementId))
        {
            return Json(new { success = false, message = "參數錯誤" });
        }

        var elements = pageEditorService.GetEditableElements(pageName, sectionName);
        var element = elements.FirstOrDefault(e => e.Id == elementId);
        
        if (element == null)
        {
            return Json(new { success = false, message = "元素不存在" });
        }

        ViewBag.Element = element;
        ViewBag.PageName = pageName;
        ViewBag.SectionName = sectionName;
        
        return PartialView("Partials/_ElementEditor");
    }

    /// <summary>
    /// 取得可編輯元素列表
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <returns>返回 JSON 結果</returns>
    [HttpGet]
    public IActionResult GetEditableElements(string pageName, string sectionName)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(sectionName))
        {
            return Json(new { success = false, elements = new List<object>() });
        }

        var elements = pageEditorService.GetEditableElements(pageName, sectionName);
        var elementsJson = elements.Select(e => new
        {
            id = e.Id,
            name = e.Name,
            type = e.Type,
            selector = e.Selector,
            value = e.Value,
            extraData = e.ExtraData
        }).ToList();
        
        return Json(new { success = true, elements = elementsJson });
    }
}

