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
        public override void Write(Report report, TextWriter @out)
        {
            CsvWriter.Write(
                @out,
                new[] {"Task", "Hours"},
                report.Items.Select(i => new[] {i.Name, i.Hours.ToString("F")}));
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            var heads = new List<string> {"id", "time", "stopped"};
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
            var csvOptions = new CsvOptions {HeaderMode = HeaderMode.HeaderPresent, TrimData = true};
            int idIndex = -1;
            int timeIndex = -1;
            int stoppedIndex = -1;
            bool first = true;
            List<TimeEntry> result = new List<TimeEntry>();
            while (reader.Peek() == '#')
                reader.ReadLine();
            foreach (var line in CsvReader.Read(reader, csvOptions))
            {
                if (first)
                {
                    var headers = new List<string>(line.Headers);
                    idIndex = headers.IndexOf("id");
                    timeIndex = headers.IndexOf("time");
                    stoppedIndex = headers.IndexOf("stopped");
                    first = false;
                }

                var id = idIndex >= 0 ? line[idIndex] : null;
                var timeString = line[timeIndex];
                var time = string.IsNullOrEmpty(timeString) ? default : DateTime.Parse(timeString);
                var stopped = stoppedIndex >= 0 ? line[stoppedIndex] : null;
                var tags = new List<string>();
                for (int i = 0; i < line.ColumnCount; i++)
                {
                    if (i != idIndex && i != timeIndex && i != stoppedIndex && !string.IsNullOrEmpty(line[i]))
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