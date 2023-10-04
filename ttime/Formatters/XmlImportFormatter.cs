using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using LiteDB;

namespace ttime.Formatters;

public class XmlImportFormatter : IImportFormatter
{
    public List<TimeEntry> DeserializeEntries(TextReader reader)
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