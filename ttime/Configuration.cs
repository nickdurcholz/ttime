using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiteDB;

namespace ttime
{
    public class Configuration
    {
        private readonly LiteDatabase _db;
        private ReportingPeriod _defaultReportingPeriod;
        private Format _defaultFormat;
        private DayOfWeek _startOfWeek;
        private decimal _roundingPrecision;

        public Configuration(LiteDatabase db)
        {
            _db = db;
            List<ConfigSetting> settings;

            string GetValue(string name, string defaultValue = null)
            {
                var setting = settings.FirstOrDefault(s => s.Name == name);
                return setting?.Value ?? defaultValue;
            }

            var collection = GetCollection();
            collection.EnsureIndex(c => c.Name, unique: true);
            settings = collection.FindAll().ToList();

            var value = GetValue("defaultReportPeriod");
            if (!Enum.TryParse(value, true, out _defaultReportingPeriod))
                _defaultReportingPeriod = ReportingPeriod.Yesterday;

            value = GetValue("defaultFormat");
            if (!Enum.TryParse(value, true, out _defaultFormat))
                _defaultFormat = Format.Text;

            value = GetValue("startOfWeek");
            if (!Enum.TryParse(value, true, out _startOfWeek))
                _startOfWeek = DayOfWeek.Monday;

            value = GetValue("rounding");
            if (!decimal.TryParse(value, out _roundingPrecision))
                _roundingPrecision = 0m;
        }

        public ReportingPeriod DefaultReportingPeriod
        {
            get => _defaultReportingPeriod;
            set
            {
                _defaultReportingPeriod = value;
                Store("defaultReportPeriod", value.ToString());
            }
        }

        public Format DefaultFormat
        {
            get => _defaultFormat;
            set
            {
                _defaultFormat = value;
                Store("defaultFormat", value.ToString());
            }
        }

        public DayOfWeek StartOfWeek
        {
            get => _startOfWeek;
            set
            {
                _startOfWeek = value;
                Store("startOfWeek", value.ToString());
            }
        }

        public decimal RoundingPrecision
        {
            get => _roundingPrecision;
            set
            {
                _roundingPrecision = value;
                Store("rounding", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void Store(string name, string value)
        {
            var collection = GetCollection();
            var setting = collection.FindOne(s => s.Name == name) ?? new ConfigSetting {Name = name};
            setting.Value = value;
            collection.Upsert(setting);
        }

        private LiteCollection<ConfigSetting> GetCollection()
        {
            return _db.GetCollection<ConfigSetting>("config");
        }

        private class ConfigSetting
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}