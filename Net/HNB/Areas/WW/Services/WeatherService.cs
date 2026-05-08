using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HNB.Areas.WW.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HNB.Areas.WW.Services
{
    public interface IWeatherService
    {
        Task<WeatherCurrentResult> QueryCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);

        Task<WeatherForecastResult> QueryForecastWeatherAsync(
            double latitude,
            double longitude,
            int forecastDays,
            CancellationToken cancellationToken = default);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        #region Public Methods

        public async Task<WeatherCurrentResult> QueryCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = WeatherUtilities.BuildCurrentWeatherCacheKey(latitude, longitude);

            if (_memoryCache.TryGetValue(cacheKey, out WeatherCurrentResult? cachedWeatherResult) &&
                cachedWeatherResult is not null)
            {
                return cachedWeatherResult;
            }

            var requestUrl = WeatherUtilities.BuildCurrentWeatherUrl(latitude, longitude);
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            var rootElement = jsonDocument.RootElement;
            var currentElement = rootElement.GetProperty("current");
            var weatherCode = currentElement.GetProperty("weather_code").GetInt32();

            var weatherCurrentResult = new WeatherCurrentResult
            {
                Temperature = currentElement.GetProperty("temperature_2m").GetDecimal(),
                RelativeHumidity = currentElement.GetProperty("relative_humidity_2m").GetInt32(),
                ApparentTemperature = currentElement.GetProperty("apparent_temperature").GetDecimal(),
                WeatherCode = weatherCode,
                WeatherDescription = WeatherUtilities.QueryWeatherDescription(weatherCode),
                LucideIconName = WeatherUtilities.QueryLucideIconName(weatherCode),
                WindSpeed = currentElement.GetProperty("wind_speed_10m").GetDecimal()
            };

            _memoryCache.Set(
                cacheKey,
                weatherCurrentResult,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return weatherCurrentResult;
        }

        public async Task<WeatherForecastResult> QueryForecastWeatherAsync(
            double latitude,
            double longitude,
            int forecastDays,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = WeatherUtilities.BuildForecastWeatherCacheKey(latitude, longitude, forecastDays);

            if (_memoryCache.TryGetValue(cacheKey, out WeatherForecastResult? cachedWeatherForecastResult) &&
                cachedWeatherForecastResult is not null)
            {
                return cachedWeatherForecastResult;
            }

            var requestUrl = WeatherUtilities.BuildForecastWeatherUrl(latitude, longitude, forecastDays);
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            var rootElement = jsonDocument.RootElement;
            var dailyElement = rootElement.GetProperty("daily");
            var timeElements = dailyElement.GetProperty("time");
            var weatherCodeElements = dailyElement.GetProperty("weather_code");
            var maxTemperatureElements = dailyElement.GetProperty("temperature_2m_max");

            var weatherForecastResult = new WeatherForecastResult();

            for (var index = 0; index < timeElements.GetArrayLength(); index++)
            {
                var dateText = timeElements[index].GetString() ?? string.Empty;
                var weatherCode = weatherCodeElements[index].GetInt32();

                weatherForecastResult.ForecastDays.Add(new WeatherForecastDayResult
                {
                    Date = DateOnly.Parse(dateText),
                    WeatherCode = weatherCode,
                    WeatherDescription = WeatherUtilities.QueryWeatherDescription(weatherCode),
                    LucideIconName = WeatherUtilities.QueryLucideIconName(weatherCode),
                    MaxTemperature = maxTemperatureElements[index].GetDecimal()
                });
            }

            _memoryCache.Set(
                cacheKey,
                weatherForecastResult,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });

            return weatherForecastResult;
        }

        #endregion
    }

    public class WeatherCurrentResult
    {
        #region Properties

        public decimal Temperature { get; set; }

        public int RelativeHumidity { get; set; }

        public decimal ApparentTemperature { get; set; }

        public int WeatherCode { get; set; }

        public string WeatherDescription { get; set; } = string.Empty;

        public string LucideIconName { get; set; } = string.Empty;

        public decimal WindSpeed { get; set; }

        #endregion
    }

    public class WeatherForecastResult
    {
        #region Properties

        public List<WeatherForecastDayResult> ForecastDays { get; set; } = [];

        #endregion
    }

    public class WeatherForecastDayResult
    {
        #region Properties

        public DateOnly Date { get; set; }

        public int WeatherCode { get; set; }

        public string WeatherDescription { get; set; } = string.Empty;

        public string LucideIconName { get; set; } = string.Empty;

        public decimal MaxTemperature { get; set; }

        #endregion
    }
}