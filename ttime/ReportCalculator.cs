﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ttime
{
    public class ReportCalculator
    {
        private readonly Storage _storage;
        private readonly ReportingPeriod _period;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private readonly DayOfWeek _startOfWeek;
        private readonly decimal _rounding;
        private readonly bool _daily;
        private readonly List<Regex> _tags;

        public ReportCalculator(
            Storage storage,
            ReportingPeriod period,
            DateTime fromDate,
            DateTime toDate,
            DayOfWeek startOfWeek,
            decimal rounding,
            bool daily = false,
            List<string> tags = null)
        {
            _storage = storage;
            _period = period;
            _fromDate = fromDate;
            _toDate = toDate;
            _startOfWeek = startOfWeek;
            _rounding = rounding;
            _daily = daily;
            _tags = tags == null
                ? new List<Regex>(0)
                : tags.Select(t => new Regex(t, RegexOptions.IgnoreCase)).ToList();
        }

        public IEnumerable<Report> CreateReport()
        {
            var (start, end) = DateTimeUtility.ExpandPeriod(_period, _startOfWeek, _fromDate, _toDate);
            if (_daily)
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

            var previousEntry = default(TimeEntry);
            var report = new Report(_rounding)
            {
                Start = start,
                End = end
            };
            foreach (var entry in entries)
            {
                if (previousEntry != null && !previousEntry.Stopped &&
                    (_tags.Count == 0 || previousEntry.Tags.Any(pt => _tags.Any(t => t.IsMatch(pt)))))
                {
                    report.Add(previousEntry.Tags, (long) (entry.Time - previousEntry.Time).TotalMilliseconds);
                }

                previousEntry = entry;
            }

            if (previousEntry != null && !previousEntry.Stopped &&
                (_tags.Count == 0 || previousEntry.Tags.Any(pt => _tags.Any(t => t.IsMatch(pt)))))
            {
                var nextEntry = _storage.GetNextEntry(previousEntry);
                var endTime = nextEntry?.Time ?? DateTime.Now;
                var currentMs = (long) (endTime - previousEntry.Time).TotalMilliseconds;
                report.Add(previousEntry.Tags, currentMs);
            }

            SortItems(report.Items);
            return report;
        }

        private void SortItems(List<Report.Item> items)
        {
            items.Sort((a,b) => StringComparer.OrdinalIgnoreCase.Compare(a.Tag, b.Tag));
            foreach (var i in items)
                SortItems(i.Children);
        }
    }
}