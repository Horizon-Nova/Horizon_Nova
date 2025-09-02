using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class SettingsController(SettingsServices svc) : Controller
{
    public IActionResult Settings(string t = "system")
    {
        var system = svc.GetVwSystemConfigSystem().FirstOrDefault();
        var security = svc.GetVwSystemConfigSecurity().FirstOrDefault();
        var notification = svc.GetVwSystemConfigNotification().FirstOrDefault();
        var database = svc.GetVwSystemConfigDatabase().FirstOrDefault();
        var server = svc.GetVwSystemConfigServer().FirstOrDefault();

        var map = new Dictionary<string, (string Partial, object? Model)>
        {
            ["system"] = ("_Settings.System", system),
            ["security"] = ("_Settings.Security", security),
            ["notification"] = ("_Settings.Notification", notification),
            ["database"] = ("_Settings.Database", database),
            ["schedule"] = ("_Settings.Schedule", null),
            ["server"] = ("_Settings.Server", server),
        };

        var tab = string.IsNullOrWhiteSpace(t) ? "system" : t.ToLowerInvariant();
        if (!map.ContainsKey(tab)) tab = "system";

        ViewBag.ActiveTab = tab;
        ViewData["CurrentPartial"] = map[tab].Partial;
        ViewData["CurrentModel"] = map[tab].Model;

        return View();
    }
}
