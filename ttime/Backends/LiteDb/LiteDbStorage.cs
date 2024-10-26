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
            throw new CommandError($"Data file is out-of-date. Please run '{new UpgradeDbCommand().Name}' to " +
                                   $"upgrade data file to the current version.");
        }
    }

    private ILiteCollection<LiteDbConfigSetting> ConfigCollection
    {
        get
        {
            if (_configCollection == null)
                _configCollection = _db.GetCollection<LiteDbConfigSetting>("config");

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

    private ILiteCollection<LiteDbAlias> AliasCollection => _aliasCollection ??= _db.GetCollection<LiteDbAlias>("alias");

    public void Dispose() => _db?.Dispose();

    public IEnumerable<ConfigSetting> ListConfigSettings() => ConfigCollection.FindAll().Select(s => s.Setting);

    public void Save(ConfigSetting setting) => ConfigCollection.Upsert(new LiteDbConfigSetting(setting));

    public void Save(TimeEntry timeEntry) => TimeCollection.Upsert(new LiteDbTimeEntry(timeEntry));

    public IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end)
    {
        //start inclusive, end exclusive. This avoids an edge case when reporting when a task starts at exactly midnight.
        return TimeCollection.Query()
                             .Where(e => start <= e.Time && e.Time < end)
                             .OrderBy(e => e.Time)
                             .ToEnumerable()
                             .Select(e => e.Entry);
    }

    public TimeEntry GetNextEntry(TimeEntry entry)
    {
        var entryTime = entry.Time;
        return TimeCollection.FindOne(e => e.Time > entryTime)?.Entry;
    }

    public void DeleteEntry(string entryId) => TimeCollection.Delete(new ObjectId(entryId));

    public void Save(IEnumerable<TimeEntry> entries) =>
        TimeCollection.Upsert(entries.Select(e => new LiteDbTimeEntry(e)));

    public IEnumerable<Alias> ListAliases() => AliasCollection.FindAll().Select(x => x.Alias);

    public void Save(Alias alias) => AliasCollection.Upsert(new LiteDbAlias(alias));

    public void Delete(Alias alias) => AliasCollection.Delete(new ObjectId(alias.Id));

    public TimeEntry this[string id] => TimeCollection.FindById(new ObjectId(id)).Entry;

    public TimeEntry GetLastEntry(int offset)
    {
        return TimeCollection
              .Find(Query.All(nameof(TimeEntry.Time), Query.Descending), offset, 1)
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
                              throw new CommandError("HOME environment variable is not set.");
                dbFolder = Path.Combine(homeDir, ".config", ".ttime");
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var homeDir = Environment.GetEnvironmentVariable("HOME") ??
                              throw new CommandError("HOME environment variable is not set.");
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
                       throw new CommandError($"Invalid TTIME_DATA environment variable: {dbPath}");
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