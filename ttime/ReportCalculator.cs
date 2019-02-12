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

        public ReportCalculator(Storage storage,
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

        public Report CreateReport()
        {
            var (start, end) = ExpandPeriod();
            var entries = _storage.ListTimeEntries(start, end);

            var times = new Dictionary<string, long>();
            var previousTags = new List<string>();
            var previousEntry = default(TimeEntry);
            long totalTime = 0;
            foreach (var entry in entries)
            {
                if (previousEntry != null && !previousEntry.Stopped)
                {
                    var currentMs = (long)(entry.Time - previousEntry.Time).TotalMilliseconds;
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
                var currentMs = (long)(endTime - previousEntry.Time).TotalMilliseconds;
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

        /// <summary>
        /// Get the starting date (inclusive) and the ending date (exclusive) for the reporting period. Filter entries on start &lt;= Time &lt; end.
        /// </summary>
        public (DateTime start, DateTime end) ExpandPeriod()
        {
            var today = DateTime.Today;
            switch (_period)
            {
                case ReportingPeriod.Sunday:
                case ReportingPeriod.Monday:
                case ReportingPeriod.Tuesday:
                case ReportingPeriod.Wednesday:
                case ReportingPeriod.Thursday:
                case ReportingPeriod.Friday:
                case ReportingPeriod.Saturday:
                {
                    int targetDay = (int) _period;
                    int currentDay = (int) today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    if (offset < 1)
                        offset += 7;
                    return (today.AddDays(-offset), today.AddDays(1 - offset));
                }
                case ReportingPeriod.LastWeek:
                {
                    int targetDay = (int) _startOfWeek;
                    int currentDay = (int) today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    return (today.AddDays(-offset - 7), today.AddDays(-offset));
                }
                case ReportingPeriod.Week:
                {
                    int targetDay = (int) _startOfWeek;
                    int currentDay = (int) today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    return (today.AddDays(-offset), today.AddDays(-offset + 7));
                }
                case ReportingPeriod.Yesterday:
                    return (today.AddDays(-1), today);
                case ReportingPeriod.Today:
                    return (today, today.AddDays(1));
                case ReportingPeriod.Custom:
                    return (_fromDate, _toDate);
                case ReportingPeriod.All:
                    return (DateTime.MinValue, DateTime.MaxValue);
                case ReportingPeriod.Month:
                    return (
                        new DateTime(today.Year, today.Month, 1),
                        new DateTime(today.Year + ((today.Month + 1) / 12), (today.Month + 1) % 12, 1)
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
    }
}