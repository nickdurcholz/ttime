using System;
using System.Collections.Generic;
using System.IO;
using Csv;

namespace ttime.Formatters;

public class CsvImportFormatter : IImportFormatter
{
    public List<TimeEntry> DeserializeEntries(TextReader reader)
    {
        var csvOptions = new CsvOptions { HeaderMode = HeaderMode.HeaderAbsent, TrimData = true };
        const int timeIndex = 0;
        const int stoppedIndex = 1;
        int lineNumber = 0;
        List<TimeEntry> result = new List<TimeEntry>();
        while (reader.Peek() == '#')
            reader.ReadLine();
        foreach (var line in CsvReader.Read(reader, csvOptions))
        {
            lineNumber++;

            if (lineNumber == 1 && line[timeIndex].EqualsOIC("time"))
                continue; // skip header line if present

            var timeString = line[timeIndex];
            DateTime time;
            try
            {
                time = string.IsNullOrEmpty(timeString) ? default : DateTime.Parse(timeString);
            }
            catch (FormatException)
            {
                throw new FormatException($"Error on line {lineNumber}. '{timeString}' is not a valid date/time.");
            }

            var stopped = line[stoppedIndex];
            var tags = new List<string>();
            for (int i = 2; i < line.ColumnCount; i++)
            {
                if (!string.IsNullOrEmpty(line[i]))
                    tags.Add(line[i]);
            }

            result.Add(new TimeEntry
            {
                Time = time,
                Tags = tags.ToArray(),
                Stopped = !string.IsNullOrEmpty(stopped) && bool.Parse(stopped)
            });
        }

        return result;
    }
}