using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ttime.Formatters;

public class JsonImportFormatter : IImportFormatter
{
    public List<TimeEntry> DeserializeEntries(TextReader reader)
    {
        using var jtr = new JsonTextReader(reader);
        return JsonSerializer.CreateDefault().Deserialize<List<TimeEntry>>(jtr);
    }
}