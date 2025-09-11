using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
[OperationPermission(requireIpMatch: true, verifyDb: true)]
public class SettingsController(SettingsServices svc) : Controller
{
    public IActionResult Settings()
        => View();

    public IActionResult SettingsTab(string t = "system")
    {
        var tab = NormalizeTab(t);
        var (view, model) = GetTabViewAndModel(tab);
        return View(view, model);
    }

    private static string NormalizeTab(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "system";
        t = t.ToLowerInvariant();
        return t is "system" or "security" or "notification" or "database" or "schedule" or "server"
            ? t : "system";
    }

    private (string View, object? Model) GetTabViewAndModel(string tab)
    {
        return tab switch
        {
            "system" => ("System", svc.GetVwSystemConfigSystem().FirstOrDefault()),
            "security" => ("Security", svc.GetVwSystemConfigSecurity().FirstOrDefault()),
            "notification" => ("Notification", svc.GetVwSystemConfigNotification().FirstOrDefault()),
            "database" => ("Database", svc.GetVwSystemConfigDatabase().FirstOrDefault()),
            "schedule" => ("Schedule", null),
            "server" => ("Server", svc.GetVwSystemConfigServer().FirstOrDefault()),
            _ => ("System", svc.GetVwSystemConfigSystem().FirstOrDefault()),
        };
    }
}
