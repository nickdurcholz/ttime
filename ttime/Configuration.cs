using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ttime
{
    public class Configuration
    {
        private readonly Storage _storage;
        private ReportingPeriod _defaultReportingPeriod;
        private Format _defaultFormat;
        private DayOfWeek _startOfWeek;
        private decimal _roundingPrecision;

        public Configuration(Storage storage)
        {
            _storage = storage;
            List<ConfigSetting> settings;

            string GetValue(string name, string defaultValue = null)
            {
                var setting = settings.FirstOrDefault(s => s.Name == name);
                return setting?.Value ?? defaultValue;
            }

            settings = storage.ListConfigSettings();

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
            var setting = _storage.FindConfigSetting(name) ?? new ConfigSetting {Name = name};
            setting.Value = value;
            _storage.Save(setting);
        }
    }
}