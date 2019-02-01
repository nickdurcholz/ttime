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

        public ReportCalculator(
            Storage storage,
            ReportingPeriod period,
            DateTime fromDate,
            DateTime toDate,
            List<string> tags,
            DayOfWeek startOfWeek)
        {
            _storage = storage;
            _period = period;
            _fromDate = fromDate;
            _toDate = toDate;
            _tags = tags;
            _startOfWeek = startOfWeek;
        }

        public Report CreateReport()
        {
            var (start, end) = ExpandPeriod();
            var entries = _storage.ListTimeEntries(start, end);

            var times = new Dictionary<string, double>();
            var previousTags = new List<string>();
            var previousEntry = default(TimeEntry);
            foreach (var entry in entries)
            {
                if (previousEntry != null && !previousEntry.Stopped)
                {
                    foreach (var previousTag in previousTags)
                    {
                        times.TryGetValue(previousTag, out var total);
                        total += (entry.Time - previousEntry.Time).TotalHours;
                        times[previousTag] = total;
                    }
                }

                if (!entry.Stopped)
                {
                    previousTags.Clear();
                    if (_tags.Count == 0)
                        previousTags.Add(entry.Tags.Length == 0 ? "Unspecified" : entry.Tags[0]);
                    else
                        previousTags.AddRange(entry.Tags.Intersect(_tags));
                }

                previousEntry = entry;
            }

            if (previousEntry != null && !previousEntry.Stopped)
            {
                foreach (var previousTag in previousTags)
                {
                    times.TryGetValue(previousTag, out var total);
                    var nextEntry = _storage.GetNextEntry(previousEntry);
                    var endTime = nextEntry?.Time ?? DateTime.Now;
                    total += (endTime - previousEntry.Time).TotalHours;
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
                Items = keys.Select(k => new Report.Item
                {
                    Name = k,
                    Hours = times[k],
                }).ToList(),
                Total = times.Values.Sum(x => x)
            };
        }

        public (DateTime start, DateTime end) ExpandPeriod()
        {
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
                    int currentDay = (int) DateTime.Today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    if (offset < 1)
                        offset += 7;
                    return (DateTime.Today.AddDays(-offset), DateTime.Today.AddDays(1 - offset));
                }
                case ReportingPeriod.LastWeek:
                {
                    int targetDay = (int) _startOfWeek;
                    int currentDay = (int) DateTime.Today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    if (offset < 1)
                        offset += 7;
                    return (DateTime.Today.AddDays(-offset - 7), DateTime.Today.AddDays(-offset));
                }
                case ReportingPeriod.Week:
                {
                    int targetDay = (int) _startOfWeek;
                    int currentDay = (int) DateTime.Today.DayOfWeek;
                    int offset = currentDay - targetDay;
                    if (offset < 1)
                        offset += 7;
                    return (DateTime.Today.AddDays(-offset), DateTime.Now);
                }
                case ReportingPeriod.Yesterday:
                    return (DateTime.Today.AddDays(-1), DateTime.Today);
                case ReportingPeriod.Today:
                    return (DateTime.Today, DateTime.Now);
                case ReportingPeriod.Custom:
                    return (_fromDate, _toDate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ReportingPeriod));
            }
        }
    }
}