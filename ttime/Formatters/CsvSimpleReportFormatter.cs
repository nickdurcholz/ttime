using System.Collections.Generic;
using System.IO;
using Csv;

namespace ttime.Formatters;

public class CsvSimpleReportFormatter : IReportFormatter
{
    private readonly TimeFormatter _timeFormatter;

    public CsvSimpleReportFormatter(TimeFormatter timeFormatter)
    {
        _timeFormatter = timeFormatter;
    }

    public void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
    {
        CsvWriter.Write(@out, new[] {"Date", "Category", "Time"}, GenerateData(reports));
    }

    private List<string[]> GenerateData(IEnumerable<Report> reports)
    {
        var result = new List<string[]>();
        foreach (var report in reports)
        foreach (var item in report.Items)
            GenerateRows(report, item, result);
        return result;
    }

    private void GenerateRows(Report report, ReportItem item, List<string[]> result)
    {
        if (item.Items.Count == 0)
            result.Add(new[]
            {
                report.Start.Date.ToString("yyyy-MM-dd"),
                item.TagLine,
                _timeFormatter.Format(item.HoursExcludingChildren)
            });
        else
        {
            foreach (var subItem in item.Items)
                GenerateRows(report, subItem, result);
        }
    }
}