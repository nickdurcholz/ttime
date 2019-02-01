using System;
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
    }
}