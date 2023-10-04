using System;

namespace ttime.Formatters;

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
        if (_format == TimeFormat.TimeSpan)
            return TimeSpan.FromHours((double) hours).ToString();

        var ts = TimeSpan.FromHours((double)hours);
        var h = (int)ts.TotalHours;
        var m = (int)Math.Round(ts.TotalMinutes) - h * 60;
        string hourMinute;
        if (h == 0)
            hourMinute = $"{m}m";
        else if (m == 0)
            hourMinute = $"{h}h";
        else
            hourMinute =$"{h}h {m}m";

        if (_format == TimeFormat.HoursMinutes)
            return hourMinute;

        return $"{hours.ToString("N")} ({hourMinute})";
    }
}