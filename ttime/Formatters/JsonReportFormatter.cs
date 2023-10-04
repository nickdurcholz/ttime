using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ttime.Formatters;

public class JsonReportFormatter : IReportFormatter
{
    public void Write(IEnumerable<Report> report, TextWriter @out, int? nestingLevel)
    {
        var ls = report as IList<Report> ?? report.ToList();
        if (ls.Count == 1)
            @out.WriteLine(JsonConvert.SerializeObject(ls[0], Formatting.Indented));
        else
            @out.WriteLine(JsonConvert.SerializeObject(ls, Formatting.Indented));
    }
}