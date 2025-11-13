using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.NovaAPP.Controllers;

[Area("NovaAPP")]
public class DialVisionController : Controller
{
    public IActionResult Index() => View();

    public IActionResult LoadScreen(string screen)
    {
        var screenPath = screen?.ToLower() switch
        {
            "login" => "Partials/DialVision/Screens/_LoginScreen",
            "dashboard" => "Partials/DialVision/Screens/_DashboardScreen",
            "register" => "Partials/DialVision/Screens/_RegisterScreen",
            "camera" => "Partials/DialVision/Screens/_CameraScreen",
            "settings" => "Partials/DialVision/Screens/_SettingsScreen",
            _ => "Partials/DialVision/Screens/_LoginScreen"
        };

        return PartialView(screenPath);
    }

    public IActionResult LoadCalendar(int? year = null, int? month = null, string? selectedDate = null)
    {
        ViewBag.Year = year ?? DateTime.Now.Year;
        ViewBag.Month = month ?? DateTime.Now.Month;
        ViewBag.SelectedDate = selectedDate;
        return PartialView("Partials/DialVision/_Calendar");
    }

    public IActionResult LoadTimeWheel(string? selectedTime = null)
    {
        ViewBag.SelectedTime = selectedTime;
        return PartialView("Partials/DialVision/_TimeWheel");
    }
}
