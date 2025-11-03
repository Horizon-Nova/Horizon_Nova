using Microsoft.AspNetCore.Mvc;
using HNB.IIS.Core.Filters;
using HNB.IIS.Core.Services;
using HNB.IIS.Core.Dtos;

namespace HNB.IIS.Core.Controllers;

/// <summary>
/// 站台管理控制器
/// </summary>
[RequireMing]
public class SiteManagementController(SiteManagementService service) : Controller
{
    /// <summary>
    /// 站台管理首頁
    /// </summary>
    /// <returns>站台管理視圖</returns>
    public IActionResult Index()
    {
        var sites = service.LoadSiteList();
        ViewBag.Sites = sites;
        return View();
    }

    /// <summary>
    /// 啟動站台
    /// </summary>
    /// <param name="request">站台操作請求</param>
    /// <returns>操作結果</returns>
    [HttpPost]
    public IActionResult Start([FromBody] SiteActionRequest request)
    {
        try
        {
            var result = service.StartSite(request.SiteName);
            return Json(new { success = result, message = result ? "站台已啟動" : "啟動失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"啟動失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 停止站台
    /// </summary>
    /// <param name="request">站台操作請求</param>
    /// <returns>操作結果</returns>
    [HttpPost]
    public IActionResult Stop([FromBody] SiteActionRequest request)
    {
        try
        {
            var result = service.StopSite(request.SiteName);
            return Json(new { success = result, message = result ? "站台已停止" : "停止失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"停止失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 刪除站台
    /// </summary>
    /// <param name="request">站台操作請求</param>
    /// <returns>操作結果</returns>
    [HttpPost]
    public IActionResult Delete([FromBody] SiteActionRequest request)
    {
        try
        {
            var result = service.DeleteSite(request.SiteName);
            return Json(new { success = result, message = result ? "站台已刪除" : "刪除失敗" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"刪除失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 建立新站台
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <param name="zipFile">上傳的 ZIP 檔案</param>
    /// <returns>操作結果</returns>
    [HttpPost]
    public IActionResult Create(string siteName, IFormFile zipFile)
    {
        if (string.IsNullOrEmpty(siteName) || zipFile == null)
        {
            return Json(new { success = false, message = "請提供站台名稱和壓縮檔" });
        }

        var result = service.CreateSiteFromZip(siteName, zipFile);
        return Json(new { success = result, message = result ? "站台已建立" : "建立失敗：請檢查壓縮檔格式是否正確" });
    }

    /// <summary>
    /// 取得站台狀態
    /// </summary>
    /// <param name="siteName">站台名稱</param>
    /// <returns>站台狀態</returns>
    [HttpGet]
    public IActionResult Status(string siteName)
    {
        var status = service.ParseSiteStatus(siteName);
        return Json(new { status });
    }
}

