using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace ttime
{
    public class Storage
    {
        private const int CurrentDataVersion = 1;

        private readonly LiteDatabase _db;
        private ILiteCollection<ConfigSetting> _configCollection;
        private ILiteCollection<TimeEntry> _timeCollection;
        private ILiteCollection<Alias> _aliasCollection;

        public Storage(LiteDatabase db)
        {
            _db = db;
        }

        private ILiteCollection<ConfigSetting> ConfigCollection
        {
            get
            {
                if (_configCollection == null)
                {
                    _configCollection = _db.GetCollection<ConfigSetting>("config");
                }

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

        public IEnumerable<ConfigSetting> ListConfigSettings()
        {
            return ConfigCollection.FindAll();
        }

        public void Save(ConfigSetting setting)
        {
            ConfigCollection.Upsert(setting);
        }

        public void Save(TimeEntry timeEntry)
        {
            TimeCollection.Upsert(timeEntry);
        }

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

        public void DeleteEntry(ObjectId entryId)
        {
            TimeCollection.Delete(entryId);
        }

        public void Save(IEnumerable<TimeEntry> entries)
        {
            TimeCollection.Upsert(entries);
        }

        public IEnumerable<Alias> ListAliases()
        {
            return AliasCollection.FindAll();
        }

        public void Save(Alias @alias)
        {
            AliasCollection.Upsert(@alias);
        }

        public void Delete(Alias @alias)
        {
            AliasCollection.Delete(@alias.Id);
        }

        public TimeEntry this[string id] => TimeCollection.FindById(new ObjectId(id));

        public TimeEntry GetLastEntry(int offset) => TimeCollection
            .Find(Query.All(nameof(TimeEntry.Time), Query.Descending), offset, 1)
            .SingleOrDefault();

        public void Import(LiteDatabase originalDb)
        {
            ConfigCollection.InsertBulk(originalDb.GetCollection<ConfigSetting>("config").FindAll());
            AliasCollection.InsertBulk(originalDb.GetCollection<Alias>("alias").FindAll());
            TimeCollection.InsertBulk(originalDb.GetCollection<TimeEntry>("time").FindAll());
            _db.GetCollection<DataFormatVersion>("data_version").Insert(new DataFormatVersion {Version = CurrentDataVersion});
        }
    }
}