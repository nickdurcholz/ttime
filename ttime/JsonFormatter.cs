using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ttime
{
    public class JsonFormatter : Formatter
    {
        public override void Write(IEnumerable<Report> report, TextWriter @out, int? nestingLevel, List<string> tags)
        {
            var ls = report as IList<Report> ?? report.ToList();
            if (ls.Count == 1)
                @out.WriteLine(JsonConvert.SerializeObject(ls[0], Formatting.Indented));
            else
                @out.WriteLine(JsonConvert.SerializeObject(ls, Formatting.Indented));
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            @out.WriteLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
        }

        public override List<TimeEntry> DeserializeEntries(TextReader reader)
        {
            using (JsonTextReader jtr = new JsonTextReader(reader))
                return JsonSerializer.CreateDefault().Deserialize<List<TimeEntry>>(jtr);
        }
    }
}