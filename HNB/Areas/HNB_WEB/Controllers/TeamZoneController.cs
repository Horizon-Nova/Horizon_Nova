using HNB.Areas.HNB_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

namespace HNB.Areas.HNB_WEB.Controllers;

[Area("HNB_WEB")]
public class TeamZoneController(TeamZoneServices sev) : Controller
{
    public IActionResult NovaHome()
        => View();
    public IActionResult Consultation()
        => View();
    public IActionResult Portfolio()
        => View();

    public IActionResult ProjectDetail(string id)
    {
        // 依 id 對應到部分視圖名稱
        var partial = (id ?? "").ToLowerInvariant() switch
        {
            "warehouse" => "_ProjectWarehouse",
            "smart-meter" => "_ProjectSmartMeter",
            "tagv" => "_ProjectTAGV",
            "ai-monitor" => "_ProjectAIMonitor",
            "missa" => "_ProjectMISSA",
            "eos" => "_ProjectEOS",

            _ => "NotFound"
        };

        return View(partial);
    }

    public IActionResult NotFound()
        => View();
}
