using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Models;
using HNB.Areas.WW.Services;
using HNB.Areas.WW.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.WW.Controllers
{
    [Area("WW")]
    public class TodayController : Controller
    {
        private readonly IWeatherService _weatherService;
        private readonly IWwAiWardrobeService _wwAiWardrobeService;
        private readonly ILogger<TodayController> _logger;

        public TodayController(
            IWeatherService weatherService,
            IWwAiWardrobeService wwAiWardrobeService,
            ILogger<TodayController> logger)
        {
            _weatherService = weatherService;
            _wwAiWardrobeService = wwAiWardrobeService;
            _logger = logger;
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

        [HttpPost]
        public async Task<IActionResult> GenerateOutfit(
            [FromBody] WwAiOutfitRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _wwAiWardrobeService.RecommendOutfitAsync(request, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> TweakOutfit(
            [FromBody] WwAiTweakOutfitRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TweakInstruction))
                return BadRequest(new { message = "請提供微調指令" });

            var result = await _wwAiWardrobeService.TweakOutfitAsync(request, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> OutfitHistory(
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _wwAiWardrobeService.GetOutfitHistoryAsync(limit, cancellationToken);
                // 沒有歷史記錄或 Qdrant 暫不可用都屬於正常狀態，一律 200
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OutfitHistory query failed");
                return Ok(new WwOutfitHistoryResult { Success = false, Items = [], Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveFuturePlan(
            [FromBody] WwFuturePlanRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Date) || string.IsNullOrWhiteSpace(request.Note))
                return BadRequest(new { message = "日期與計畫說明不可為空" });

            try
            {
                var result = await _wwAiWardrobeService.SaveFuturePlanAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SaveFuturePlan failed. Date={Date}", request.Date);
                // Qdrant 不可用時仍回 200，讓前端 futPlans 保留記憶體版本
                return Ok(new WwFuturePlanResult
                {
                    Date    = request.Date,
                    Note    = request.Note,
                    SavedAt = DateTime.UtcNow.ToString("O")
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FuturePlans(CancellationToken cancellationToken)
        {
            try
            {
                var plans = await _wwAiWardrobeService.GetFuturePlansAsync(cancellationToken);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FuturePlans query failed");
                // Qdrant 不可用時回傳空陣列，前端降級為記憶體模式
                return Ok(new List<WwFuturePlanResult>());
            }
        }

        #endregion
    }
}
