using System;

namespace HNB.Areas.WW.Utilities
{
    public static class WeatherUtilities
    {
        #region Public Methods

        public static string BuildCurrentWeatherUrl(double latitude, double longitude)
        {
            var normalizedLatitude = NormalizeLatitude(latitude);
            var normalizedLongitude = NormalizeLongitude(longitude);

            return
                $"https://api.open-meteo.com/v1/forecast" +
                $"?latitude={normalizedLatitude}" +
                $"&longitude={normalizedLongitude}" +
                $"&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m" +
                $"&timezone=auto";
        }

        public static string BuildForecastWeatherUrl(double latitude, double longitude, int forecastDays)
        {
            var normalizedLatitude = NormalizeLatitude(latitude);
            var normalizedLongitude = NormalizeLongitude(longitude);
            var normalizedForecastDays = NormalizeForecastDays(forecastDays);

            return
                $"https://api.open-meteo.com/v1/forecast" +
                $"?latitude={normalizedLatitude}" +
                $"&longitude={normalizedLongitude}" +
                $"&daily=weather_code,temperature_2m_max" +
                $"&forecast_days={normalizedForecastDays}" +
                $"&timezone=auto";
        }

        public static string BuildCurrentWeatherCacheKey(double latitude, double longitude)
        {
            var normalizedLatitude = Math.Round(NormalizeLatitude(latitude), 4);
            var normalizedLongitude = Math.Round(NormalizeLongitude(longitude), 4);

            return $"WW:Weather:Current:{normalizedLatitude}:{normalizedLongitude}";
        }

        public static string BuildForecastWeatherCacheKey(double latitude, double longitude, int forecastDays)
        {
            var normalizedLatitude = Math.Round(NormalizeLatitude(latitude), 4);
            var normalizedLongitude = Math.Round(NormalizeLongitude(longitude), 4);
            var normalizedForecastDays = NormalizeForecastDays(forecastDays);

            return $"WW:Weather:Forecast:{normalizedLatitude}:{normalizedLongitude}:{normalizedForecastDays}";
        }

        public static string QueryWeatherDescription(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "晴朗",
                1 => "大致晴朗",
                2 => "局部多雲",
                3 => "陰天",
                45 => "霧",
                48 => "霜霧",
                51 => "毛毛雨",
                53 => "中度毛毛雨",
                55 => "強毛毛雨",
                56 => "凍毛毛雨",
                57 => "強凍毛毛雨",
                61 => "小雨",
                63 => "中雨",
                65 => "大雨",
                66 => "凍雨",
                67 => "強凍雨",
                71 => "小雪",
                73 => "中雪",
                75 => "大雪",
                77 => "雪粒",
                80 => "短暫陣雨",
                81 => "中度陣雨",
                82 => "強陣雨",
                85 => "短暫陣雪",
                86 => "強陣雪",
                95 => "雷雨",
                96 => "雷雨夾小冰雹",
                99 => "雷雨夾大冰雹",
                _ => "未知天氣"
            };
        }

        public static string QueryLucideIconName(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "sun",
                1 or 2 => "cloud-sun",
                3 => "cloud",
                45 or 48 => "cloud-fog",
                51 or 53 or 55 or 56 or 57 or 61 or 63 or 66 or 80 or 81 => "cloud-drizzle",
                65 or 67 or 82 => "cloud-rain",
                71 or 73 or 75 or 77 or 85 or 86 => "cloud-snow",
                95 or 96 or 99 => "cloud-lightning",
                _ => "cloud"
            };
        }

        public static string QueryWeatherEmoji(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "☀️",
                1 or 2 => "🌤️",
                3 => "☁️",
                45 or 48 => "🌫️",
                51 or 53 or 55 or 56 or 57 => "🌦️",
                61 or 63 or 65 or 66 or 67 or 80 or 81 or 82 => "🌧️",
                71 or 73 or 75 or 77 or 85 or 86 => "🌨️",
                95 or 96 or 99 => "⛈️",
                _ => "☁️"
            };
        }

        public static string QueryWeekdayText(DateOnly date, DateOnly today)
        {
            return date == today
                ? "Today"
                : date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Mon",
                    DayOfWeek.Tuesday => "Tue",
                    DayOfWeek.Wednesday => "Wed",
                    DayOfWeek.Thursday => "Thu",
                    DayOfWeek.Friday => "Fri",
                    DayOfWeek.Saturday => "Sat",
                    DayOfWeek.Sunday => "Sun",
                    _ => string.Empty
                };
        }

        #endregion

        #region Private Methods

        private static double NormalizeLatitude(double latitude)
        {
            return latitude > 90
                ? 90
                : latitude < -90
                    ? -90
                    : latitude;
        }

        private static double NormalizeLongitude(double longitude)
        {
            return longitude > 180
                ? 180
                : longitude < -180
                    ? -180
                    : longitude;
        }

        private static int NormalizeForecastDays(int forecastDays)
        {
            return forecastDays < 1
                ? 1
                : forecastDays > 16
                    ? 16
                    : forecastDays;
        }

        #endregion
    }
}