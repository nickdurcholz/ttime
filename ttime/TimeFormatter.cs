using System;

namespace ttime
{
    public class TimeFormatter
    {
        private readonly TimeFormat _format;

        public TimeFormatter(TimeFormat format)
        {
            _format = format;
        }

        public string Format(decimal hours)
        {
            if (_format == TimeFormat.DecimalHours)
                return hours.ToString("N");

            var ts = TimeSpan.FromHours((double)hours);
            var h = (int)ts.TotalHours;
            var m = (int)Math.Round(ts.TotalMinutes) - h * 60;
            if (h == 0)
                return $"{m}m";
            if (m == 0)
                return $"{h}h";
            return $"{h}h {m}m";
        }
    }

    public enum TimeFormat
    {
        DecimalHours,
        HoursMinutes
    }
}