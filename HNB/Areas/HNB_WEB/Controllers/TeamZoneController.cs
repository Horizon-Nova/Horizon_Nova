using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HNB_WEB.Controllers;

/// <summary>
/// 團隊區域控制器，負責處理團隊相關的頁面顯示功能
/// 包括首頁、諮詢頁面、專案詳情和作品集展示
/// </summary>
[Area("HNB_WEB")]
public class TeamZoneController(TeamZoneServices sev) : Controller
{
    /// <summary>
    /// 顯示 Nova 首頁
    /// </summary>
    /// <returns>返回首頁視圖</returns>
    public IActionResult NovaHome()
    => View();
    
    /// <summary>
    /// 顯示諮詢頁面
    /// </summary>
    /// <returns>返回諮詢頁面視圖</returns>
    public IActionResult Consultation()
        => View();

    /// <summary>
    /// 顯示特定專案的詳細資訊
    /// </summary>
    /// <param name="id">專案ID</param>
    /// <returns>返回包含專案詳情的視圖，如果找不到專案則返回空模型</returns>
    public IActionResult ProjectDetail(int id)
    {
        var model = sev.ProjectDetailData(id);
        return View(model);
    }

    /// <summary>
    /// 顯示作品集頁面
    /// 載入專案標籤、專案資料和團隊成員資訊到 ViewBag
    /// </summary>
    /// <returns>返回作品集視圖，包含所有相關資料</returns>
    public IActionResult Portfolio()
    {
        sev.ViewBagModelPortfolio(ViewBag);
        return View();
    }
}
