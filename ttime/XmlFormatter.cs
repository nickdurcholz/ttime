using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ttime
{
    public class XmlFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            var reportElement = new XElement(
                "report",
                new XAttribute("start", report.Start.ToString("u")),
                new XAttribute("end", report.End.ToString("u")),
                new XAttribute("total", report.Total));
            foreach (var item in report.Items)
            {
                reportElement.Add(new XElement(
                    "task",
                    new XAttribute("name", item.Name),
                    new XAttribute("hours", item.Hours)));
            }

            using (var writer = XmlWriter.Create(@out, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                reportElement.WriteTo(writer);
                writer.Flush();
                writer.Close();
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
                    new XAttribute("time", entry.Time.ToString("O")),
                    new XAttribute("stopped", entry.Stopped ? "true" : "false"));
                foreach (var tag in entry.Tags)
                    entryElement.Add(new XElement("task", tag));
                entriesElement.Add(entryElement);
            }

            using (var writer = XmlWriter.Create(@out, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                entriesElement.WriteTo(writer);
                writer.Flush();
                writer.Close();
            }
        }
    }
}