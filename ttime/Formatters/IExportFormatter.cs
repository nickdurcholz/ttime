using System.Collections.Generic;
using System.IO;

namespace ttime.Formatters;

public interface IExportFormatter
{
    void Write(IEnumerable<TimeEntry> entries, TextWriter @out);
}