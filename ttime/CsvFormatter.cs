using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csv;

namespace ttime
{
    public class CsvFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            CsvWriter.Write(
                @out,
                new [] {"Task","Hours"},
                report.Items.Select(i => new [] { i.Name, i.Hours.ToString("F")}));
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            var heads = new List<string> { "id", "time", "stopped" };
            var data = entries.ToList();
            var numTags = data.Count == 0 ? 0 : data.Max(e => e.Tags.Length);

            heads.AddRange(Enumerable.Range(0, numTags).Select(i => "task" + i));
            CsvWriter.Write(
                @out,
                heads.ToArray(),
                data.Select(e => new [] { e.Id.ToString(), e.Time.ToString("O"), e.Stopped ? "true" : "false" }.Concat(e.Tags).ToArray()));
        }
    }
}