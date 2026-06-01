namespace HNB.Areas.WW.Models
{
    public class WeatherDayModel
    {
        #region Properties

        public string DateText { get; set; } = string.Empty;

        public string WeekdayText { get; set; } = string.Empty;

        public string DateFullText { get; set; } = string.Empty;

        public string WeatherIconEmoji { get; set; } = string.Empty;

        public string WeatherDescriptionText { get; set; } = string.Empty;

        public string TemperatureText { get; set; } = string.Empty;

        public string DayType { get; set; } = string.Empty;

        public bool IsToday { get; set; }

        public bool IsSelected { get; set; }

        public bool IsCompleted { get; set; }

        #endregion
    }
}
