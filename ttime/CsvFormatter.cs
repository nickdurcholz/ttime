using System;
using System.Collections.Generic;
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
            var rows = reports.SelectMany(r => GetRows(r)).ToList();
            var maxcols = rows.Max(r => r.Length);
            var headers = new List<string> { "Report Start", "Report End", "Hours" };
            for (int i = 0; i < maxcols-3; i++)
            {
                headers.Add($"Tag {i}");
            }
            CsvWriter.Write(@out, headers.ToArray(), rows);
        }

        private IEnumerable<string[]> GetRows(Report report)
        {
            Stack<Report.Item> stack = new Stack<Report.Item>();
            var itemsInScope = EnumerateItems(report.Items);
            return itemsInScope.Select(i =>
            {
                var list = new List<string>
                {
                    report.Start.ToString("O"),
                    report.End.ToString("O"),
                    i.Hours.ToString("F")
                };
                list.AddRange(EnumerateTags(i, stack));
                return list.ToArray();
            });
        }

        private IEnumerable<string> EnumerateTags(Report.Item item, Stack<Report.Item> stack)
        {
            while (item != null)
            {
                stack.Push(item);
                item = item.Parent;
            }

            if (stack.Count == 0)
                yield return "Unspecified";
            while (stack.Count > 0)
                yield return stack.Pop().Tag;
        }

        private IEnumerable<Report.Item> EnumerateItems(List<Report.Item> items)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var c in EnumerateItems(item.Children))
                    yield return c;
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