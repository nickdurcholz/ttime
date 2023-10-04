using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ttime.Formatters;

public class JsonExportFormatter : IExportFormatter
{
    public void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
    {
        @out.WriteLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
    }
}