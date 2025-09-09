using HNB.Areas.HnbBackoffice.Filters;
using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Mvc;

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
        var (partial, model) = GetTabPartialAndModel(tab);
        return PartialView(partial, model);
    }

    private static string NormalizeTab(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "system";
        t = t.ToLowerInvariant();
        return t is "system" or "security" or "notification" or "database" or "schedule" or "server"
            ? t
            : "system";
    }

    private (string Partial, object? Model) GetTabPartialAndModel(string tab)
    {
        return tab switch
        {
            "system" => ("_Settings.System", svc.GetVwSystemConfigSystem().FirstOrDefault()),
            "security" => ("_Settings.Security", svc.GetVwSystemConfigSecurity().FirstOrDefault()),
            "notification" => ("_Settings.Notification", svc.GetVwSystemConfigNotification().FirstOrDefault()),
            "database" => ("_Settings.Database", svc.GetVwSystemConfigDatabase().FirstOrDefault()),
            "schedule" => ("_Settings.Schedule", null),
            "server" => ("_Settings.Server", svc.GetVwSystemConfigServer().FirstOrDefault()),
            _ => ("_Settings.System", svc.GetVwSystemConfigSystem().FirstOrDefault()),
        };
    }
}
