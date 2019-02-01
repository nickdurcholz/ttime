using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace ttime
{
    public class Storage
    {
        private readonly LiteDatabase _db;
        private LiteCollection<ConfigSetting> _configCollection;
        private LiteCollection<TimeEntry> _timeCollection;

        public Storage(LiteDatabase db)
        {
            _db = db;
        }

        private LiteCollection<ConfigSetting> ConfigCollection
        {
            get
            {
                if (_configCollection == null)
                {
                    _configCollection = _db.GetCollection<ConfigSetting>("config");
                    _configCollection.EnsureIndex(c => c.Name, true);
                }

                return _configCollection;
            }
        }

        private LiteCollection<TimeEntry> TimeCollection
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

        public ConfigSetting FindConfigSetting(string name)
        {
            return ConfigCollection.FindOne(s => s.Name == name);
        }

        public List<ConfigSetting> ListConfigSettings()
        {
            return ConfigCollection.FindAll().ToList();
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
    }
}