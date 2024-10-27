using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csv;

namespace ttime.Formatters;

public class CsvExportFormatter : IExportFormatter
{
    public void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
    {
        var heads = new List<string> { "time", "stopped" };
        var data = entries.ToList();
        var numTags = data.Count == 0 ? 0 : data.Where(e => e.Tags != null).Max(e => e.Tags.Length);

        heads.AddRange(Enumerable.Range(0, numTags).Select(i => "tag" + i));
        CsvWriter.Write(
            @out,
            heads.ToArray(),
            data.Select(e =>
            {
                var tagCount = e.Tags?.Length ?? 0;
                var row = new string[tagCount + 2];

                row[0] = e.Time.ToString("O");
                row[1] = e.Stopped ? "true" : "false";

                for (int i = 0; i < tagCount; i++)
                    row[2 + i] = e.Tags[i] ?? string.Empty;

                return row;
            }));
    }
}