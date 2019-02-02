using System;
using System.Collections.Generic;
using LiteDB;

namespace ttime
{
    public class Storage
    {
        private readonly LiteDatabase _db;
        private LiteCollection<ConfigSetting> _configCollection;
        private LiteCollection<TimeEntry> _timeCollection;
        private LiteCollection<Alias> _aliasCollection;

        public Storage(LiteDatabase db)
        {
            _db = db;
        }

        private LiteCollection<ConfigSetting> ConfigCollection =>
            _configCollection ?? (_configCollection = _db.GetCollection<ConfigSetting>("config"));

        private LiteCollection<TimeEntry> TimeCollection
        {
            get
            {
                if (_timeCollection == null)
                {
                    _timeCollection = _db.GetCollection<TimeEntry>("time");
                    _timeCollection.EnsureIndex(e => e.Time, true);
                }

                return _timeCollection;
            }
        }

        private LiteCollection<Alias> AliasCollection =>
            _aliasCollection ?? (_aliasCollection = _db.GetCollection<Alias>("alias"));

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
            return TimeCollection.Find(e => start <= e.Time && e.Time < end);
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
    }
}