using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace ttime;

public class LiteDbStorage : IStorage, IDisposable
{
    private const int CurrentDataVersion = 1;

    private readonly LiteDatabase _db;
    private ILiteCollection<Alias> _aliasCollection;
    private ILiteCollection<ConfigSetting> _configCollection;
    private ILiteCollection<TimeEntry> _timeCollection;

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

    private ILiteCollection<ConfigSetting> ConfigCollection
    {
        get
        {
            if (_configCollection == null)
                _configCollection = _db.GetCollection<ConfigSetting>("config");

            return _configCollection;
        }
    }

    private ILiteCollection<TimeEntry> TimeCollection
    {
        get
        {
            if (_timeCollection == null)
            {
                _timeCollection = _db.GetCollection<TimeEntry>("time");
                _timeCollection.EnsureIndex(e => e.Time);
            }

            return _timeCollection;
        }
    }

    private ILiteCollection<Alias> AliasCollection => _aliasCollection ??= _db.GetCollection<Alias>("alias");

    public void Dispose() => _db?.Dispose();

    public IEnumerable<ConfigSetting> ListConfigSettings() => ConfigCollection.FindAll();

    public void Save(ConfigSetting setting) => ConfigCollection.Upsert(setting);

    public void Save(TimeEntry timeEntry) => TimeCollection.Upsert(timeEntry);

    public IEnumerable<TimeEntry> ListTimeEntries(DateTime start, DateTime end)
    {
        //start inclusive, end exclusive. This avoids an edge case when reporting when a task starts at exactly midnight.
        var results = TimeCollection.Query()
                                    .Where(e => start <= e.Time && e.Time < end)
                                    .OrderBy(e => e.Time)
                                    .ToEnumerable();
        return results;
    }

    public TimeEntry GetNextEntry(TimeEntry entry)
    {
        var entryTime = entry.Time;
        return TimeCollection.FindOne(e => e.Time > entryTime);
    }

    public void DeleteEntry(ObjectId entryId) => TimeCollection.Delete(entryId);

    public void Save(IEnumerable<TimeEntry> entries) => TimeCollection.Upsert(entries);

    public IEnumerable<Alias> ListAliases() => AliasCollection.FindAll();

    public void Save(Alias alias) => AliasCollection.Upsert(alias);

    public void Delete(Alias alias) => AliasCollection.Delete(alias.Id);

    public TimeEntry this[string id] => TimeCollection.FindById(new ObjectId(id));

    public TimeEntry GetLastEntry(int offset)
    {
        return TimeCollection
              .Find(Query.All(nameof(TimeEntry.Time), Query.Descending), offset, 1)
              .SingleOrDefault();
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
        ConfigCollection.InsertBulk(originalDb.GetCollection<ConfigSetting>("config").FindAll());
        AliasCollection.InsertBulk(originalDb.GetCollection<Alias>("alias").FindAll());
        TimeCollection.InsertBulk(originalDb.GetCollection<TimeEntry>("time").FindAll());
        _db.GetCollection<DataFormatVersion>("data_version")
           .Insert(new DataFormatVersion { Version = CurrentDataVersion });
    }
}