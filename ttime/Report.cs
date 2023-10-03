using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace ttime;

public class Report : TimeContainer
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public void Add(IReadOnlyList<string> tags, long milliseconds)
    {
        tags ??= new[] { UnspecifiedTag };
        var items = Items;
        ReportItem parent = null;
        foreach (var tag in tags)
        {
            var item = GetOrCreateItem(tag, items, parent);
            item.Milliseconds += milliseconds;
            items = item.Items;
            parent = item;
        }

        Milliseconds += milliseconds;
    }

    private ReportItem GetOrCreateItem(string tag, List<ReportItem> items, ReportItem parent)
    {
        var item = items.FirstOrDefault(i => i.Tag == tag);
        if (item == null)
        {
            item = new ReportItem { Tag = tag, Parent = parent };
            items.Add(item);
        }

        return item;
    }
}

[DebuggerDisplay("{Tag} - {Hours}")]
public class ReportItem : TimeContainer
{
    public string Tag { get; set; }

    [JsonIgnore]
    public ReportItem Parent { get; set; }

    public string TagLine => Parent == null || Parent.TagLine == null ? Tag : $"{Parent.TagLine} {Tag}";
}

public class TimeContainer
{
    private const decimal MsPerHour = 3600000m;
    protected const string UnspecifiedTag = "unspecified";
    private long _roundToNearestMs = 1;

    public List<ReportItem> Items { get; } = new();
    public long Milliseconds { get; set; }
    public long MillisecondsExcludingChildren => Math.Max(0L, Milliseconds - Items.Sum(i => i.Milliseconds));
    public decimal HoursExcludingChildren => GetRoundedMilliseconds(MillisecondsExcludingChildren, _roundToNearestMs) / MsPerHour;
    public decimal Hours { get; set; }

    /// <summary>
    ///     Rounds time spent on this item and all children in a way that ensures total rounding error for all categories is
    ///     &lt;= roundingFactor. The total rounding error may be carried over from other reports.
    /// </summary>
    /// <param name="roundingError">Total rounding error in milliseconds</param>
    /// <param name="roundingFactor">Desired precision in fractional hours</param>
    /// <returns>Returns the total rounding error in milliseconds</returns>
    public long SetRoundedHours(long roundingError, decimal roundingFactor) => SetRoundedHours(roundingError, (long)(MsPerHour * Math.Abs(roundingFactor)));

    private long SetRoundedHours(long roundingError, long roundToNearestMs)
    {
        //This function takes the mental gymnastics out of entering accurate-as-possible time into a system that requires you
        //to do silly things like round to the nearest quarter hour.
        //
        //This rounding method differs from simple midpoint rounding in that it tries to make sure that the accumulated error
        //for the entire report is as small as possible. For example, if you are rounding to the nearest 15 minutes, then
        //simply using the midpoint rounding rule may round 10 entries down and only two up, making it seem as if you are
        //reporting up to an hour less time than was actually recorded.
        //
        //This method feeds the rounding error from one entry into the next so that we can make a better decision to round up
        //or down. Using this method, then the total rounding error for a report would be guaranteed to be less than 15 minutes
        //for the entire report.
        _roundToNearestMs = roundToNearestMs;

        foreach (var child in Items)
            roundingError = child.SetRoundedHours(roundingError, roundToNearestMs);

        if (roundToNearestMs > 0)
        {
            var timeExcludingChildren = Math.Max(0L, Milliseconds - Items.Sum(i => i.Milliseconds));
            var roundedMs = GetRoundedMilliseconds(timeExcludingChildren, roundToNearestMs);
            var myError = roundedMs - timeExcludingChildren;
            if (roundingError + myError > roundToNearestMs)
                roundedMs -= roundToNearestMs;
            else if (roundingError + myError < -roundToNearestMs)
                roundedMs += roundToNearestMs;

            Hours = roundedMs / MsPerHour + Items.Sum(i => i.Hours);

            return roundingError + roundedMs - timeExcludingChildren;
        }

        Hours = Milliseconds / MsPerHour;
        return 0;
    }

    private static long GetRoundedMilliseconds(long ms, long roundToNearestMs)
    {
        var floor = ms / roundToNearestMs * roundToNearestMs;
        var midpoint = roundToNearestMs / 2;
        if (ms - floor > midpoint)
            return floor + roundToNearestMs;
        return floor;
    }
}