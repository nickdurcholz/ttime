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
        public override void Write(IEnumerable<Report> reports, TextWriter @out)
        {
            CsvWriter.Write(
                @out,
                new[] { "Report Start", "Report End", "Task", "Hours" },
                reports.SelectMany(r => r.Items.Select(i => new[]
                {
                    r.Start.ToString("O"),
                    r.End.ToString("O"),
                    i.Name,
                    i.Hours.ToString("F")
                })));
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