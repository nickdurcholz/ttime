using System;
using System.Collections.Generic;

namespace ttime;

public interface IStorage
{
    IEnumerable<ConfigSetting> ListConfigSettings();
    void Save(ConfigSetting setting);
    void Save(TimeEntry timeEntry);
    void Save(IEnumerable<TimeEntry> entries);
    void Save(Alias alias);
    IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end);
    TimeEntry GetNextEntry(TimeEntry entry);
    void DeleteEntry(string entryId);
    IEnumerable<Alias> ListAliases();
    void Delete(Alias alias);
    TimeEntry this[string id] { get; }
    TimeEntry GetLastEntry(int offset);
}