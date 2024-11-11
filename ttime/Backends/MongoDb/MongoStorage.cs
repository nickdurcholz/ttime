using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Driver;

namespace ttime.Backends.MongoDb;

public class MongoStorage : IStorage
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private IMongoCollection<MongoTimeEntry> _timeEntries;
    private IMongoCollection<MongoConfigSetting> _configSettings;
    private IMongoCollection<MongoAlias> _aliases;

    public MongoStorage(string connectionString, string databaseName)
    {
        _client = new MongoClient(connectionString);
        _db = _client.GetDatabase(databaseName);
    }

    public IMongoCollection<MongoTimeEntry> TimeEntries => _timeEntries ??= _db.GetCollection<MongoTimeEntry>("time");

    public IMongoCollection<MongoConfigSetting> ConfigSettings
    {
        get => _configSettings ??= _db.GetCollection<MongoConfigSetting>("config_settings");
    }

    public IMongoCollection<MongoAlias> Aliases => _aliases ??= _db.GetCollection<MongoAlias>("aliases");

    public IEnumerable<ConfigSetting> ListConfigSettings() => ConfigSettings
                                                             .Find(Builders<MongoConfigSetting>.Filter.Empty)
                                                             .ToEnumerable()
                                                             .Select(c => c.Setting);

    public void Save(ConfigSetting setting)
    {
        ConfigSettings.ReplaceOne(
            Builders<MongoConfigSetting>.Filter.Eq(x => x._id, setting.Key),
            new MongoConfigSetting(setting),
            new ReplaceOptions { IsUpsert = true });
    }

    public void Save(TimeEntry timeEntry, DateTime? newTime = null)
    {
        var mongoTimeEntry = new MongoTimeEntry(timeEntry);
        if (newTime != null && newTime.Value != timeEntry.Time)
        {
            if (TimeEntries.CountDocuments(Builders<MongoTimeEntry>.Filter.Eq(x => x._id, mongoTimeEntry._id)) > 0)
                throw new TTimeError("There is already an entry with the same time.");

            var id = timeEntry.Time.ToUnixTime();
            TimeEntries.DeleteOne(Builders<MongoTimeEntry>.Filter.Eq(x => x._id, id));
            timeEntry.Time = newTime.Value;
        }

        TimeEntries.ReplaceOne(
            Builders<MongoTimeEntry>.Filter.Eq(x => x._id, mongoTimeEntry._id),
            mongoTimeEntry,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public void Save(IEnumerable<TimeEntry> entries)
    {
        TimeEntries.BulkWrite(entries.Select(e =>
        {
            var mongoEntry = new MongoTimeEntry(e);
            return new ReplaceOneModel<MongoTimeEntry>(
                Builders<MongoTimeEntry>.Filter.Eq("_id", mongoEntry._id),
                mongoEntry)
            {
                IsUpsert = true
            };
        }));
    }

    public void Save(Alias alias)
    {
        Aliases.ReplaceOne(
            Builders<MongoAlias>.Filter.Eq(x => x._id, alias.Name),
            new MongoAlias(alias),
            new ReplaceOptions { IsUpsert = true });
    }

    public IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end)
    {
        var startUnixTime = start.ToUnixTime();
        var endUnixTime = end.ToUnixTime();
        return TimeEntries.Find(e => e._id >= startUnixTime && e._id < endUnixTime)
                          .ToEnumerable()
                          .Select(x => x.Entry);
    }

    public TimeEntry GetNextEntry(TimeEntry entry)
    {
        var ut = entry.Time.ToUnixTime();
        return TimeEntries.Find(e => e._id >= ut)
                          .SortBy(e => e._id)
                          .FirstOrDefault()
                         ?.Entry;
    }

    public void DeleteEntries(IList<DateTime> timestamp)
    {
        if (timestamp.Count == 0)
            return;
        var ut = timestamp.Select(t => t.ToUnixTime());
        TimeEntries.DeleteMany(Builders<MongoTimeEntry>.Filter.In(x => x._id, ut));
    }

    public IEnumerable<Alias> ListAliases() => Aliases.Find(Builders<MongoAlias>.Filter.Empty)
                                                      .ToEnumerable()
                                                      .Select(a => a.Alias);

    public void Delete(Alias alias) => Aliases.DeleteOne(a => a._id == alias.Name);

    public TimeEntry this[DateTime timestamp]
    {
        get
        {
            var ut = timestamp.ToUnixTime();
            return TimeEntries.Find(e => e._id == ut).FirstOrDefault()?.Entry;
        }
    }

    public TimeEntry GetLastEntry(int offset) => TimeEntries.Find(FilterDefinition<MongoTimeEntry>.Empty)
                                                            .SortByDescending(e => e._id)
                                                            .Skip(offset)
                                                            .FirstOrDefault()
                                                           ?.Entry;

    public void Dispose() => _client.Dispose();
}