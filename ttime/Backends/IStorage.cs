using System;
using System.Collections.Generic;

namespace ttime.Backends;

public interface IStorage : IDisposable
{
    IEnumerable<ConfigSetting> ListConfigSettings();
    void Save(ConfigSetting setting);
    void Save(TimeEntry timeEntry, DateTime? newTime = null);
    void Save(IEnumerable<TimeEntry> entries);
    void Save(Alias alias);
    /// <summary>
    /// Lists entries with a Time >= start and < End
    /// </summary>
    IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end);
    TimeEntry GetNextEntry(TimeEntry entry);
    void DeleteEntries(IList<DateTime> timestamp);
    IEnumerable<Alias> ListAliases();
    void Delete(Alias alias);
    TimeEntry this[DateTime timestamp] { get; }
    TimeEntry GetLastEntry(int offset);
}