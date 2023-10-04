using System;
using System.Collections.Generic;
using System.IO;
using Csv;
using LiteDB;

namespace ttime.Formatters;

public class CsvImportFormatter : IImportFormatter
{
    public List<TimeEntry> DeserializeEntries(TextReader reader)
    {
        var csvOptions = new CsvOptions { HeaderMode = HeaderMode.HeaderAbsent, TrimData = true };
        const int idIndex = 0;
        const int timeIndex = 1;
        const int stoppedIndex = 2;
        int lineNumber = 0;
        List<TimeEntry> result = new List<TimeEntry>();
        while (reader.Peek() == '#')
            reader.ReadLine();
        foreach (var line in CsvReader.Read(reader, csvOptions))
        {
            lineNumber++;

            if (lineNumber == 1 && line[idIndex].EqualsOIC("id") && line[timeIndex].EqualsOIC("time"))
                continue; // skip header line if present

            var id = line[idIndex];
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
            for (int i = 3; i < line.ColumnCount; i++)
            {
                if (!string.IsNullOrEmpty(line[i]))
                    tags.Add(line[i]);
            }

            result.Add(new TimeEntry
            {
                Id = string.IsNullOrEmpty(id) ? null : new ObjectId(id),
                Time = time,
                Tags = tags.ToArray(),
                Stopped = !string.IsNullOrEmpty(stopped) && bool.Parse(stopped)
            });
        }

        return result;
    }
}