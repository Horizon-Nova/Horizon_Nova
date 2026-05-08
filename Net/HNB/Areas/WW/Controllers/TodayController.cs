using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Models;
using HNB.Areas.WW.Services;
using HNB.Areas.WW.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class TodayController : Controller
    {
        private readonly IWeatherService _weatherService;

        public TodayController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        #region Public Methods

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var latitude = 25.0330;
            var longitude = 121.5654;

            var currentWeatherResult = await _weatherService.QueryCurrentWeatherAsync(
                latitude,
                longitude,
                cancellationToken);

            var forecastWeatherResult = await _weatherService.QueryForecastWeatherAsync(
                latitude,
                longitude,
                6,
                cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var todayModel = new TodayIndexModel
            {
                CurrentLocationText = "Now · Auto-detected",
                CurrentTemperatureText = $"{currentWeatherResult.Temperature:0}°",
                CurrentWeatherSummaryText =
                    $"{currentWeatherResult.WeatherDescription} · {currentWeatherResult.RelativeHumidity}% humidity · Feels like {currentWeatherResult.ApparentTemperature:0}°",
                CurrentWindSpeedText = $"Wind {currentWeatherResult.WindSpeed:0} km/h",
                CurrentWeatherIconName = string.IsNullOrWhiteSpace(currentWeatherResult.LucideIconName)
                    ? "cloud"
                    : currentWeatherResult.LucideIconName,
                CurrentWeatherEmoji = WeatherUtilities.QueryWeatherEmoji(currentWeatherResult.WeatherCode)
            };

            foreach (var forecastWeatherDay in forecastWeatherResult.ForecastDays.Take(6))
            {
                var isToday = forecastWeatherDay.Date == today;

                todayModel.WeatherDays.Add(new WeatherDayModel
                {
                    DateText = forecastWeatherDay.Date.ToString("MM/dd"),
                    WeekdayText = WeatherUtilities.QueryWeekdayText(forecastWeatherDay.Date, today),
                    DateFullText = forecastWeatherDay.Date.ToDateTime(TimeOnly.MinValue).ToString("ddd MMM dd"),
                    WeatherIconEmoji = WeatherUtilities.QueryWeatherEmoji(forecastWeatherDay.WeatherCode),
                    WeatherDescriptionText = forecastWeatherDay.WeatherDescription,
                    TemperatureText = $"{forecastWeatherDay.MaxTemperature:0}°",
                    DayType = isToday ? "today" : "future",
                    IsToday = isToday,
                    IsSelected = isToday,
                    IsCompleted = false
                });
            }

            ViewData["Title"] = "Today";
            ViewData["TopbarBrandPrefix"] = "Whatever the ";
            ViewData["TopbarBrandEmphasis"] = "Weather.";
            ViewData["TopbarLocation"] = $"Taipei · {DateTime.Today:ddd MMM dd}";

            return View(todayModel);
        }

        #endregion
    }
}
