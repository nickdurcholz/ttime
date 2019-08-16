using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace ttime
{
    public class ReportCalculator
    {
        private readonly Storage _storage;
        private readonly ReportingPeriod _period;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private readonly List<string> _tags;
        private readonly DayOfWeek _startOfWeek;
        private readonly decimal _rounding;
        private readonly ReportType _reportType;

        public ReportCalculator(
            Storage storage,
            ReportingPeriod period,
            DateTime fromDate,
            DateTime toDate,
            List<string> tags,
            DayOfWeek startOfWeek,
            decimal rounding,
            ReportType reportType)
        {
            _storage = storage;
            _period = period;
            _fromDate = fromDate;
            _toDate = toDate;
            _tags = tags;
            _startOfWeek = startOfWeek;
            _rounding = rounding;
            _reportType = reportType;
        }

        public IEnumerable<Report> CreateReport()
        {
            var (start, end) = DateTimeUtility.ExpandPeriod(_period, _startOfWeek, _fromDate, _toDate);
            if (_reportType == ReportType.Daily)
            {
                var currentStart = start;
                var currentEnd = start.Date.AddDays(1);
                if (currentEnd > end)
                    currentEnd = end;

                while (currentStart < end)
                {
                    yield return CreateSingleReport(currentStart, currentEnd);
                    currentStart = currentEnd;
                    currentEnd = currentEnd.AddDays(1);
                    if (currentEnd > end)
                        currentEnd = end;
                }
            }
            else
            {
                yield return CreateSingleReport(start, end);
            }
        }

        private Report CreateSingleReport(DateTime start, DateTime end)
        {
            var entries = _storage.ListTimeEntries(start, end);

            var times = new Dictionary<string, long>();
            var previousTags = new List<string>();
            var previousEntry = default(TimeEntry);
            long totalTime = 0;
            foreach (var entry in entries)
            {
                if (previousEntry != null && !previousEntry.Stopped)
                {
                    var currentMs = (long) (entry.Time - previousEntry.Time).TotalMilliseconds;
                    totalTime += currentMs;
                    foreach (var previousTag in previousTags)
                    {
                        times.TryGetValue(previousTag, out var total);
                        total += currentMs;
                        times[previousTag] = total;
                    }
                }

                if (!entry.Stopped)
                {
                    previousTags.Clear();
                    if (_tags.Count == 0)
                    {
                        if (_reportType == ReportType.FirstTag)
                            previousTags.Add(entry.Tags.Length == 0 ? "Unspecified" : entry.Tags[0]);
                        else
                        {
                            if (entry.Tags.Length == 0)
                                previousTags.Add("Unspecified");
                            else
                                previousTags.AddRange(entry.Tags);
                        }
                    }
                    else
                        previousTags.AddRange(entry.Tags.Intersect(_tags));
                }

                previousEntry = entry;
            }

            if (previousEntry != null && !previousEntry.Stopped)
            {
                var nextEntry = _storage.GetNextEntry(previousEntry);
                var endTime = nextEntry?.Time ?? DateTime.Now;
                var currentMs = (long) (endTime - previousEntry.Time).TotalMilliseconds;
                totalTime += currentMs;
                foreach (var previousTag in previousTags)
                {
                    times.TryGetValue(previousTag, out var total);
                    total += currentMs;
                    times[previousTag] = total;
                }
            }

            var keys = new List<string>(_tags);
            if (keys.Count == 0)
            {
                keys.AddRange(times.Keys);
                keys.Sort(StringComparer.OrdinalIgnoreCase);
            }

            return new Report
            {
                Start = start,
                End = end,
                Items = keys.Select(k =>
                {
                    times.TryGetValue(k, out var ms);
                    return new Report.Item
                    {
                        Name = k,
                        Hours = RoundMillisecondsToHours(ms),
                    };
                }).ToList(),
                Total = RoundMillisecondsToHours(totalTime)
            };
        }

        private decimal RoundMillisecondsToHours(long ms)
        {
            if (_rounding == 0m)
                return ms / 3600000m;

            var roundingFactor = (long) (3600000 * _rounding);
            ms = ms / roundingFactor * roundingFactor;
            return ms / 3600000m;
        }
    }
}