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

    public IMongoCollection<MongoTimeEntry> TimeEntries
    {
        get
        {
            if (_timeEntries == null)
            {
                _timeEntries = _db.GetCollection<MongoTimeEntry>("ttime");
                _timeEntries.Indexes.CreateOne(new CreateIndexModel<MongoTimeEntry>(
                                                   Builders<MongoTimeEntry>.IndexKeys.Ascending(a => a.Time),
                                                   new CreateIndexOptions { Unique = true }));
            }

            return _timeEntries;
        }
    }

    public IMongoCollection<MongoConfigSetting> ConfigSettings
    {
        get { return _configSettings ??= _db.GetCollection<MongoConfigSetting>("config-settings"); }
    }

    public IMongoCollection<MongoAlias> Aliases => _aliases ??= _db.GetCollection<MongoAlias>("aliases");

    public IEnumerable<ConfigSetting> ListConfigSettings() => ConfigSettings
                                                             .Find(Builders<MongoConfigSetting>.Filter.Empty)
                                                             .ToEnumerable()
                                                             .Select(c => c.Setting);

    public void Save(ConfigSetting setting)
    {
        ConfigSettings.ReplaceOne(
            Builders<MongoConfigSetting>.Filter.Eq(x => x.Key, setting.Key),
            new MongoConfigSetting(setting),
            new ReplaceOptions { IsUpsert = true });
    }

    public void Save(TimeEntry timeEntry, DateTime? newTime = null)
    {
        var id = timeEntry.Time.ToUnixTime();
        if (newTime != null) timeEntry.Time = newTime.Value;
        var result = TimeEntries.ReplaceOne(
            Builders<MongoTimeEntry>.Filter.Eq(x => x._id, id),
            new MongoTimeEntry(timeEntry),
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
            Builders<MongoAlias>.Filter.Eq(x => x.Name, alias.Name),
            new MongoAlias(alias),
            new ReplaceOptions { IsUpsert = true });
    }

    public IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end)
    {
        return TimeEntries.Find(e => e.Time >= start && e.Time < end).ToEnumerable().Select(x => x.Entry);
    }

    public TimeEntry GetNextEntry(TimeEntry entry) => TimeEntries.Find(e => e.Time >= entry.Time)
                                                                 .SortBy(e => e.Time)
                                                                 .FirstOrDefault()
                                                                ?.Entry;

    public void DeleteEntries(IList<DateTime> timestamp)
    {
        if (timestamp.Count == 0)
            return;
        TimeEntries.DeleteMany(Builders<MongoTimeEntry>.Filter.In(x => x.Time, timestamp));
    }

    public IEnumerable<Alias> ListAliases() => Aliases.Find(Builders<MongoAlias>.Filter.Empty)
                                                      .ToEnumerable()
                                                      .Select(a => a.Alias);

    public void Delete(Alias alias) => Aliases.DeleteOne(a => a.Name == alias.Name);

    public TimeEntry this[DateTime timestamp] => TimeEntries.Find(e => e.Time == timestamp).FirstOrDefault()?.Entry;

    public TimeEntry GetLastEntry(int offset) => TimeEntries.Find(FilterDefinition<MongoTimeEntry>.Empty)
                                                            .SortByDescending(e => e.Time)
                                                            .Skip(offset)
                                                            .FirstOrDefault()
                                                           ?.Entry;

    public void Dispose() => _client.Dispose();
}