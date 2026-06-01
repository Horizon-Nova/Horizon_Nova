using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DashboardController(SettingsServices settingsService) : BaseController
{
    /// <summary>
    /// 後台儀表板首頁
    /// </summary>
    public IActionResult Index()
    {
        var hardwareInfo = settingsService.LoadLocalHardwareInfo();
        
        ViewBag.ServerStatus = hardwareInfo != null && hardwareInfo.is_active == true ? "正常運行" : "停止";
        ViewBag.CpuUsage = hardwareInfo?.cpu_usage_percent ?? 0;
        ViewBag.MemoryUsage = hardwareInfo?.memory_usage_percent ?? 0;

        return View();
    }
}
