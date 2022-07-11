using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ttime
{
    public class TextFormatter : Formatter
    {
        private readonly TimeFormatter _timeFormatter;

        public TextFormatter(TimeFormatter timeFormatter)
        {
            _timeFormatter = timeFormatter;
        }

        public override void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
        {
            foreach(var report in reports)
                Write(report, @out, nestingLevel);
        }

        private void Write(Report report, TextWriter @out, int? maxNesting)
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

                List<(string name, decimal hours, int nesting)> lines = new List<(string name, decimal hours, int nesting)>();

                if (maxNesting <= 0)
                {
                    lines.AddRange(EnumerateLeaves(report.Items, 0, new List<string>()));
                }
                else
                {
                    foreach (var item in report.Items)
                        lines.AddRange(GetHeirarchicalLines(item, maxNesting));
                }

                int maxNameLength = lines.Max(l => l.name.Length);
                int hoursStartsAt = (maxNameLength / 2) * 2 + 2;

                int maxLineLength = maxNameLength;
                foreach (var line in lines)
                {
                    var p = hoursStartsAt + line.nesting * 2;
                    @out.WriteAndPad(line.name, p);
                    var hours = _timeFormatter.Format(line.hours);
                    @out.WriteLine(hours);

                    maxLineLength = Math.Max(maxLineLength, p + hours.Length);
                }

                for (int i = 0; i < maxLineLength; i++)
                    @out.Write('-');
                @out.WriteLine();
                @out.WriteAndPad("Total", hoursStartsAt);
                @out.WriteLine(_timeFormatter.Format(report.Hours));
                @out.WriteLine();
            }
        }

        private IEnumerable<(string name, decimal hours, int nesting)> GetHeirarchicalLines(ReportItem item, int? maxNesting, int nestingLevel = 0)
        {
            var names = new List<string> {item.Tag};
            var current = item;
            while (current.Items.Count == 1 && current.Items[0].Milliseconds == current.Milliseconds)
            {
                current = current.Items[0];
                names.Add(current.Tag);
            }

            yield return (new string(' ', nestingLevel * 2) + string.Join(' ', names), current.Hours, nestingLevel);
            if (maxNesting != null && nestingLevel >= maxNesting - 1)
            {
                foreach (var result in EnumerateLeaves(current.Items, nestingLevel + 1, new List<string>()))
                    yield return result;
            }
            else
            {
                foreach (var result in current.Items.SelectMany(c => GetHeirarchicalLines(c, maxNesting, nestingLevel + 1)))
                    yield return result;
            }
        }

        private IEnumerable<(string name, decimal hours, int nesting)> EnumerateLeaves(List<ReportItem> children, int nestingLevel, List<string> names)
        {
            foreach (var c in children)
            {
                names.Add(c.Tag);
                if (c.Items.Count == 0)
                {
                    yield return (new string(' ', nestingLevel * 2) + string.Join(' ', names), c.Hours, nestingLevel);
                }
                else
                {
                    foreach (var result in EnumerateLeaves(c.Items, nestingLevel, names))
                        yield return result;
                }
                names.RemoveAt(names.Count - 1);
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