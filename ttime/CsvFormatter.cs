using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Csv;
using LiteDB;

namespace ttime
{
    public class CsvFormatter : Formatter
    {
        public override void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
        {
            var headers = new List<string> { "Tags", "Hours" };
            var data = new SortedList<string, List<decimal>>();
            int i = 0;
            foreach (var report in reports)
            {
                var reportPeriod = report.End - report.Start == TimeSpan.FromDays(1) && report.Start == report.Start.Date
                    ? report.Start.ToString("yyyy-MM-dd")
                    : $"{report.Start:yyyy-MM-dd HH:mm:ss} to {report.End:yyyy-MM-dd HH:mm:ss}";
                headers.Add(reportPeriod);
                PopulateRows(report, data, i++);
            }

            var rows = data.Select(kvp => new[] { kvp.Key }.Concat(kvp.Value.Select(h => Math.Round(h, 2).ToString(CultureInfo.CurrentCulture))).ToArray());
            CsvWriter.Write(@out, headers.ToArray(), rows);
        }

        private void PopulateRows(Report report, SortedList<string, List<decimal>> rowData, int index)
        {
            var itemsInScope = EnumerateItems(report.Items);
            foreach (var item in itemsInScope)
            {
                var tagLine = item.TagLine;
                if (!rowData.TryGetValue(tagLine, out var hours))
                {
                    hours = new List<decimal>();
                    rowData.Add(tagLine, hours);
                }
                for (int i = 0; i < index; i++)
                    hours.Add(0m);
                hours.Add(item.HoursExcludingChildren);
            }
        }

        private IEnumerable<ReportItem> EnumerateItems(IEnumerable<ReportItem> items)
        {
            foreach (var item in items)
            {
                foreach (var c in EnumerateItems(item.Items))
                    yield return c;
                if (item.MillisecondsExcludingChildren > 0)
                    yield return item;
            }
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            var heads = new List<string> { "id", "time", "stopped" };
            var data = entries.ToList();
            var numTags = data.Count == 0 ? 0 : data.Where(e => e.Tags != null).Max(e => e.Tags.Length);

            heads.AddRange(Enumerable.Range(0, numTags).Select(i => "tag" + i));
            CsvWriter.Write(
                @out,
                heads.ToArray(),
                data.Select(e =>
                {
                    var tagCount = e.Tags?.Length ?? 0;
                    var row = new string[tagCount + 3];
                    row[0] = e.Id.ToString();
                    row[1] = e.Time.ToString("O");
                    row[2] = e.Stopped ? "true" : "false";

                    for (int i = 0; i < tagCount; i++)
                        row[3 + i] = e.Tags[i] ?? string.Empty;

                    return row;
                }));
        }

        public override List<TimeEntry> DeserializeEntries(TextReader reader)
        {
            var csvOptions = new CsvOptions { HeaderMode = HeaderMode.HeaderAbsent, TrimData = true };
            const int idIndex = 0;
            const int timeIndex = 1;
            const int stoppedIndex = 2;
            int lineNumber = 0;
            List<TimeEntry> result = new List<TimeEntry>();
            while (reader.Peek() == '#')
                reader.ReadLine();
            foreach (var line in CsvReader.Read(reader, csvOptions))
            {
                lineNumber++;

                if (lineNumber == 1 && line[idIndex].EqualsOIC("id") && line[timeIndex].EqualsOIC("time"))
                    continue; // skip header line if present

                var id = line[idIndex];
                var timeString = line[timeIndex];
                DateTime time;
                try
                {
                    time = string.IsNullOrEmpty(timeString) ? default : DateTime.Parse(timeString);
                }
                catch (FormatException)
                {
                    throw new FormatException($"Error on line {lineNumber}. '{timeString}' is not a valid date/time.");
                }

                var stopped = line[stoppedIndex];
                var tags = new List<string>();
                for (int i = 3; i < line.ColumnCount; i++)
                {
                    if (!string.IsNullOrEmpty(line[i]))
                        tags.Add(line[i]);
                }

                result.Add(new TimeEntry
                {
                    Id = string.IsNullOrEmpty(id) ? null : new ObjectId(id),
                    Time = time,
                    Tags = tags.ToArray(),
                    Stopped = !string.IsNullOrEmpty(stopped) && bool.Parse(stopped)
                });
            }

            return result;
        }
    }
}