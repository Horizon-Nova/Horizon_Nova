using System.Collections.Generic;

namespace HNB.Areas.WW.Models
{
    public class TodayIndexModel
    {
        #region Properties

        public string CurrentLocationText { get; set; } = string.Empty;

        public string CurrentTemperatureText { get; set; } = string.Empty;

        public string CurrentWeatherSummaryText { get; set; } = string.Empty;

        public string CurrentWindSpeedText { get; set; } = string.Empty;

        public string CurrentWeatherIconName { get; set; } = string.Empty;

        public string CurrentWeatherEmoji { get; set; } = string.Empty;

        public List<WeatherDayModel> WeatherDays { get; set; } = [];

        #endregion
    }
}
