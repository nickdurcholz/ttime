using System;
using System.Collections.Generic;
using System.IO;

namespace ttime;

public abstract class Formatter
{
    public static Formatter Create(OutputFormat format, TimeFormat timeFormat)
    {
        switch (format)
        {
            case OutputFormat.Text:
                return new TextFormatter(new TimeFormatter(timeFormat));
            case OutputFormat.Csv:
                return new CsvFormatter();
            case OutputFormat.Xml:
                return new XmlFormatter();
            case OutputFormat.Json:
                return new JsonFormatter();
            default:
                return null;
        }
    }

    public abstract void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel);

    public abstract void Write(IEnumerable<TimeEntry> entries, TextWriter @out);

    public abstract List<TimeEntry> DeserializeEntries(TextReader reader);
}