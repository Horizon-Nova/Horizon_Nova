using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class SettingsController : Controller
{
    public IActionResult Settings() => View();
}
