using System.Collections.Generic;
using System.IO;

namespace ttime.Formatters;

public interface IImportFormatter
{
    List<TimeEntry> DeserializeEntries(TextReader reader);
}