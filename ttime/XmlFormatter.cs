using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LiteDB;

namespace ttime;

public class XmlFormatter : Formatter
{
    public override void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
    {
        var elements = reports.Select(CreateReportElement).ToArray();
        var root = elements.Length == 1 ? elements[0] : new XElement("reports", elements);

        var settings = new XmlWriterSettings {Indent = true, OmitXmlDeclaration = true};
        using (var writer = XmlWriter.Create(@out, settings))
        {
            root.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }
    }

    private static XElement CreateReportElement(Report report)
    {
        var reportElement = new XElement(
            "report",
            new XAttribute("start", report.Start.ToString("s")),
            new XAttribute("end", report.End.ToString("s")),
            new XAttribute("hours", report.Hours));
        AddSubTasks(report.Items, reportElement);

        return reportElement;
    }

    private static void AddSubTasks(List<ReportItem> items, XElement container)
    {
        foreach (var item in items)
        {
            var task = new XElement(
                "task",
                new XAttribute("name", item.Tag),
                new XAttribute("hours", item.Hours));
            container.Add(task);
            AddSubTasks(item.Items, task);
        }
    }

    public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
    {
        var entriesElement = new XElement("ttime");
        foreach (var entry in entries)
        {
            var entryElement = new XElement(
                "entry",
                new XAttribute("id", entry.Id),
                new XAttribute("time", entry.Time.ToString("s")),
                new XAttribute("stopped", entry.Stopped ? "true" : "false"));
            foreach (var tag in entry.Tags)
                entryElement.Add(new XElement("tag", tag));
            entriesElement.Add(entryElement);
        }

        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
        using (var writer = XmlWriter.Create(@out, settings))
        {
            entriesElement.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }
    }

    public override List<TimeEntry> DeserializeEntries(TextReader reader)
    {
        var xd = XDocument.Load(reader);
        var result = new List<TimeEntry>();
        foreach (var entryElement in xd.Root.Elements("entry"))
        {
            var id = entryElement.Attribute("id")?.Value;
            var time = entryElement.Attribute("time")?.Value;
            var stopped = entryElement.Attribute("stopped")?.Value;

            var tags = new List<string>();
            foreach (var tagElement in entryElement.Elements("tag"))
                tags.Add(tagElement.Value);

            result.Add(new TimeEntry
            {
                Id = string.IsNullOrEmpty(id) ? null : new ObjectId(id),
                Time = DateTime.Parse(time),
                Tags = tags.ToArray(),
                Stopped = !string.IsNullOrEmpty(stopped) && bool.Parse(stopped)
            });
        }

        return result;
    }
}