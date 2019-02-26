using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ttime
{
    public class TextFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            if (report.Items.Count == 0)
            {
                @out.WriteLine($"No time was logged for the period {report.Start:F} - {report.End:F}.");
            }
            else
            {
                @out.Write("Hours logged for the period ");
                @out.Write(report.Start.ToString("F"));
                @out.Write(" - ");
                @out.Write(report.End.ToString("F"));
                @out.WriteLine(":");

                var itemWidth = ((report.Items.Max(i => Math.Max(5, i.Name.Length) + 2) / 4) + 1) * 4;
                foreach (var item in report.Items)
                {
                    @out.Write("  ");
                    @out.WriteAndPad(item.Name, itemWidth - 2);
                    @out.WriteLine(item.Hours.ToString("N"));
                }

                @out.WriteLine();

                @out.WriteAndPad("  Total", itemWidth);
                @out.WriteLine(report.Total.ToString("N"));
            }
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            foreach (var entry in entries)
            {
                @out.Write(entry.Time.ToString("s"));
                @out.Write("  ");
                if (entry.Stopped)
                {
                    @out.Write("Stopped");
                }
                else
                {
                    for (var i = 0; i < entry.Tags.Length; i++)
                    {
                        @out.Write(entry.Tags[i]);
                        if (i != entry.Tags.Length - 1)
                            @out.Write(", ");
                    }
                }

                @out.WriteLine();
            }
        }

        public override List<TimeEntry> DeserializeEntries(TextReader reader)
        {
            throw new NotImplementedException();
        }
    }
}