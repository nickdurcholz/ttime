using System;
using System.Text.RegularExpressions;

namespace ttime;

public static class DateTimeUtility
{
    public static bool TryParseDateOffset(string offset, DateTime date, out DateTime result)
    {
        var match = Regex.Match(offset, @"(?i)^(?<n>-?\d+(\.\d+)?)(?<unit>h|m|s)$");
        if (match.Success)
        {
            switch (match.Groups["unit"].Value.ToLowerInvariant())
            {
                case "h":
                    result = DateTime.Now.AddHours(double.Parse(match.Groups["n"].Value));
                    break;
                case "m":
                    result = DateTime.Now.AddMinutes(double.Parse(match.Groups["n"].Value));
                    break;
                case "s":
                    result = DateTime.Now.AddSeconds(double.Parse(match.Groups["n"].Value));
                    break;
                default:
                    result = default;
                    return false;
            }

            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Get the starting date (inclusive) and the ending date (exclusive) for the reporting period. Filter entries on start &lt;= Time &lt; end.
    /// </summary>
    public static (DateTime start, DateTime end) ExpandPeriod(
        ReportingPeriod period,
        DayOfWeek startOfWeek,
        DateTime customStart,
        DateTime customEnd)
    {
        var today = DateTime.Today;
        switch (period)
        {
            case ReportingPeriod.Sunday:
            case ReportingPeriod.Monday:
            case ReportingPeriod.Tuesday:
            case ReportingPeriod.Wednesday:
            case ReportingPeriod.Thursday:
            case ReportingPeriod.Friday:
            case ReportingPeriod.Saturday:
            {
                int targetDay = (int)period;
                int currentDay = (int)today.DayOfWeek;
                int offset = currentDay - targetDay;
                if (offset < 1)
                    offset += 7;
                return (today.AddDays(-offset), today.AddDays(1 - offset));
            }
            case ReportingPeriod.LastWeek:
            {
                int targetDay = (int)startOfWeek;
                int currentDay = (int)today.DayOfWeek;
                int offset = currentDay - targetDay;
                return (today.AddDays(-offset - 7), today.AddDays(-offset));
            }
            case ReportingPeriod.Week:
            {
                int targetDay = (int)startOfWeek;
                int currentDay = (int)today.DayOfWeek;
                int offset = currentDay - targetDay;
                return (today.AddDays(-offset), today.AddDays(-offset + 7));
            }
            case ReportingPeriod.Yesterday:
                return (today.AddDays(-1), today);
            case ReportingPeriod.Today:
                return (today, today.AddDays(1));
            case ReportingPeriod.Custom:
                return (customStart, customEnd);
            case ReportingPeriod.All:
                return (DateTime.MinValue, DateTime.MaxValue);
            case ReportingPeriod.Month:
                return (
                    new DateTime(today.Year, today.Month, 1),
                    new DateTime(today.Year + today.Month / 12, (today.Month + 1) % 13, 1)
                );
            case ReportingPeriod.Quarter:
            {
                var q = today.Month / 4;
                var startMonth = q * 3 + 1;
                var nextStartMonth = (q + 1) * 3 + 1;
                return (
                    new DateTime(today.Year, startMonth, 1),
                    new DateTime(today.Year + (nextStartMonth / 12), nextStartMonth % 12, 1)
                );
            }
            case ReportingPeriod.Year:
                return (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1));
            default:
                throw new ArgumentOutOfRangeException(nameof(ReportingPeriod));
        }
    }

    public static readonly long Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks /
                                        TimeSpan.TicksPerMillisecond;

    public static long ToUnixTime(this DateTime dt)
    {
        return dt.ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond - Epoch;
    }

    public static DateTime UnixTimeToLocalTime(long unixTime)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTime).ToLocalTime();
    }
}