using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbStorage : IStorage, IDisposable
{
    private const int CurrentDataVersion = 1;

    private readonly LiteDatabase _db;
    private ILiteCollection<LiteDbAlias> _aliasCollection;
    private ILiteCollection<LiteDbConfigSetting> _configCollection;
    private ILiteCollection<LiteDbTimeEntry> _timeCollection;

    public LiteDbStorage(LiteDatabase db)
    {
        _db = db;
    }

    public LiteDbStorage()
    {
        try
        {
            _db = new LiteDatabase(new ConnectionString
            {
                Filename = GetDbPath(),
                Upgrade = false
            });
        }
        catch (LiteException ex) when (ex.ErrorCode == 103)
        {
            throw new TTimeError($"Data file is out-of-date. Please run '{new UpgradeDbCommand().Name}' to " +
                                 $"upgrade data file to the current version.");
        }
    }

    private ILiteCollection<LiteDbConfigSetting> ConfigCollection
    {
        get
        {
            if (_configCollection == null)
                _configCollection = _db.GetCollection<LiteDbConfigSetting>("config");
            _configCollection.EnsureIndex(e => e.Key);

            return _configCollection;
        }
    }

    private ILiteCollection<LiteDbTimeEntry> TimeCollection
    {
        get
        {
            if (_timeCollection == null)
            {
                _timeCollection = _db.GetCollection<LiteDbTimeEntry>("time");
                _timeCollection.EnsureIndex(e => e.Time);
            }

            return _timeCollection;
        }
    }

    private ILiteCollection<LiteDbAlias> AliasCollection
    {
        get
        {
            if (_aliasCollection == null)
            {
                _aliasCollection = _db.GetCollection<LiteDbAlias>("alias");
                _aliasCollection.EnsureIndex(e => e.Name);
            }

            return _aliasCollection;
        }
    }

    public void Dispose() => _db?.Dispose();

    public IEnumerable<ConfigSetting> ListConfigSettings() => ConfigCollection.FindAll().Select(s => s.Setting);

    public void Save(ConfigSetting setting)
    {
        var c = ConfigCollection.FindOne(s => s.Key == setting.Key) ?? new LiteDbConfigSetting(setting);
        c.Setting = setting;
        ConfigCollection.Upsert(c);
    }

    public void Save(ttime.TimeEntry timeEntry) => TimeCollection.Upsert(new LiteDbTimeEntry(timeEntry));

    public IEnumerable<ttime.TimeEntry> ListTimeEntries(DateTime start, DateTime end)
    {
        return TimeCollection.Query()
                             .Where(e => start <= e.Time && e.Time < end)
                             .OrderBy(e => e.Time)
                             .ToEnumerable()
                             .Select(e => e.Entry);
    }

    public ttime.TimeEntry GetNextEntry(ttime.TimeEntry entry)
    {
        var entryTime = entry.Time;
        return TimeCollection.FindOne(e => e.Time > entryTime)?.Entry;
    }

    public void DeleteEntries(IList<DateTime> timestamp)
    {
        TimeCollection.DeleteMany(Query.Or(
            timestamp.Select(ts => Query.EQ(nameof(LiteDbTimeEntry.Time), ts)).ToArray()
        ));
    }

    public void Save(IEnumerable<ttime.TimeEntry> entries)
    {
        var orderedEntries = entries.OrderBy(e => e.Time).ToList();
        if (orderedEntries.Count == 0)
            return;
        var minTime = orderedEntries.First().Time;
        var maxTime = orderedEntries.Last().Time;
        var existingEntries = TimeCollection.Query()
                                            .Where(e => e.Time >= minTime && e.Time <= maxTime)
                                            .OrderBy(e => e.Time)
                                            .ToList();
        var toSave = new List<LiteDbTimeEntry>();
        var i = 0;
        foreach (var entry in orderedEntries)
        {
            bool add = true;
            while (i < existingEntries.Count)
            {
                var existing = existingEntries[i];
                if (existing.Time > entry.Time)
                    break;
                i++;
                if (entry.Time == existing.Time)
                {
                    existing.Entry = entry;
                    toSave.Add(existing);
                    add = false;
                    break;
                }
            }
            if (add)
                toSave.Add(new LiteDbTimeEntry(entry));
        }
        TimeCollection.Upsert(toSave);
    }

    public IEnumerable<Alias> ListAliases() => AliasCollection.FindAll().Select(x => x.Alias);

    public void Save(Alias alias)
    {
        var a = AliasCollection.FindOne(a => a.Name == alias.Name) ?? new LiteDbAlias(alias);
        a.Alias = alias;
        AliasCollection.Upsert(a);
    }

    public void Delete(Alias alias) => AliasCollection.DeleteMany(a => a.Name == alias.Name);

    public TimeEntry this[DateTime timestamp] => TimeCollection.FindOne(e => e.Time == timestamp)?.Entry;

    public ttime.TimeEntry GetLastEntry(int offset)
    {
        return TimeCollection
              .Find(Query.All(nameof(ttime.TimeEntry.Time), Query.Descending), offset, 1)
              .SingleOrDefault()
             ?.Entry;
    }

    public static string GetDbPath()
    {
        var dbPath = Environment.GetEnvironmentVariable("TTIME_DATA");
        string dbFolder;
        if (string.IsNullOrEmpty(dbPath))
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var homeDir = Environment.GetEnvironmentVariable("HOME") ??
                              throw new TTimeError("HOME environment variable is not set.");
                dbFolder = Path.Combine(homeDir, ".config", "ttime");
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var homeDir = Environment.GetEnvironmentVariable("HOME") ??
                              throw new TTimeError("HOME environment variable is not set.");
                dbFolder = Path.Combine(homeDir, "Library", "Application Support", "ttime");
            }
            else
            {
                dbFolder = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create);
                dbFolder = Path.Combine(dbFolder, "ttime");
            }

            dbPath = Path.Combine(dbFolder, "data.litedb");
        }
        else
        {
            dbFolder = Path.GetDirectoryName(dbPath) ??
                       throw new TTimeError($"Invalid TTIME_DATA environment variable: {dbPath}");
        }

        if (!Directory.Exists(dbFolder))
            Directory.CreateDirectory(dbFolder);
        return dbPath;
    }

    public void Import(LiteDatabase originalDb)
    {
        ConfigCollection.InsertBulk(originalDb.GetCollection<LiteDbConfigSetting>("config").FindAll());
        AliasCollection.InsertBulk(originalDb.GetCollection<LiteDbAlias>("alias").FindAll());
        TimeCollection.InsertBulk(originalDb.GetCollection<LiteDbTimeEntry>("time").FindAll());
        _db.GetCollection<DataFormatVersion>("data_version")
           .Insert(new DataFormatVersion { Version = CurrentDataVersion });
    }
}