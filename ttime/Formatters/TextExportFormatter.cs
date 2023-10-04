using System.Collections.Generic;
using System.IO;

namespace ttime.Formatters;

public class TextExportFormatter : IExportFormatter
{
    public void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
    {
        foreach (var entry in entries)
        {
            @out.Write(entry.Time.ToString("s"));
            @out.Write("  ");
            if (entry.Stopped)
                @out.Write("Stopped");
            else
                for (var i = 0; i < entry.Tags.Length; i++)
                {
                    @out.Write(entry.Tags[i]);
                    if (i != entry.Tags.Length - 1)
                        @out.Write(", ");
                }

            @out.WriteLine();
        }
    }
}