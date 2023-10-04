using System.Collections.Generic;
using System.IO;

namespace ttime.Formatters;

public interface IReportFormatter
{
    void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel);
}