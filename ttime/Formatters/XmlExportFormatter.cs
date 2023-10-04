using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ttime.Formatters;

public class XmlExportFormatter : IExportFormatter
{
    public void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
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

        var settings = new XmlWriterSettings {Indent = true, OmitXmlDeclaration = true};
        using (var writer = XmlWriter.Create(@out, settings))
        {
            entriesElement.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }
    }
}