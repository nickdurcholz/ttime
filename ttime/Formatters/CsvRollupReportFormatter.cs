using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Csv;

namespace ttime.Formatters;

public class CsvRollupReportFormatter : IReportFormatter
{
    public void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
    {
        var headers = new List<string> { "Tags" };
        var data = new SortedList<string, List<decimal?>>();
        int i = 0;
        var totalsRow = new List<string>();
        totalsRow.Add("Total");
        foreach (var report in reports)
        {
            var reportPeriod = report.End - report.Start == TimeSpan.FromDays(1) && report.Start == report.Start.Date
                ? report.Start.ToString("yyyy-MM-dd")
                : $"{report.Start:yyyy-MM-dd HH:mm:ss} to {report.End:yyyy-MM-dd HH:mm:ss}";
            headers.Add(reportPeriod + " rollup");
            headers.Add(reportPeriod);
            PopulateRows(report, data, i++);
            totalsRow.Add("");
            totalsRow.Add(FormatData(report.Hours));
        }

        var k = data.Max(kvp => kvp.Value.Count);
        foreach (var kvp in data)
        {
            for (i = kvp.Value.Count; i < k; i++)
                kvp.Value.Add(null);
        }

        string FormatData(decimal? h) => h is null or 0
            ? ""
            : Math.Round(h.Value, 2).ToString(CultureInfo.CurrentCulture);
        var rows = data
            .Select(kvp => new[] { kvp.Key }.Concat(kvp.Value.Select(FormatData)).ToArray())
            .Concat(new[] { totalsRow.ToArray() });
        CsvWriter.Write(@out, headers.ToArray(), rows);
    }

    private void PopulateRows(Report report, SortedList<string, List<decimal?>> rowData, int reportIndex)
    {
        var itemsInScope = EnumerateItems(report.Items);

        foreach (var item in itemsInScope)
        {
            var tagLine = item.TagLine;
            if (!rowData.TryGetValue(tagLine, out var hours))
            {
                hours = new List<decimal?>();
                rowData.Add(tagLine, hours);
            }

            for (int i = hours.Count; i < reportIndex; i++)
            {
                hours.Add(null);
                hours.Add(null);
            }

            hours.Add(item.Hours);
            hours.Add(item.HoursExcludingChildren);
        }
    }

    private IEnumerable<ReportItem> EnumerateItems(IEnumerable<ReportItem> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var c in EnumerateItems(item.Items))
                yield return c;
        }
    }
}