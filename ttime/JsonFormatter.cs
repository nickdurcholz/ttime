using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ttime
{
    public class JsonFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            @out.WriteLine(JsonConvert.SerializeObject(report, Formatting.Indented));
        }

        public override void Write(IEnumerable<TimeEntry> entries, TextWriter @out)
        {
            @out.WriteLine(JsonConvert.SerializeObject(entries, Formatting.Indented));
        }
    }
}