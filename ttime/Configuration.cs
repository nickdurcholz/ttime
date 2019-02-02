﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ttime
{
    public class Configuration
    {
        private const string DefaultReportingPeriodKey = "defaultReportPeriod";
        private const string DefaultReportFormatKey = "defaultReportFormat";
        private const string DefaultImportFormatKey = "defaultImportFormat";
        private const string StartOfWeekKey = "startOfWeek";
        private const string RoundingKey = "rounding";

        private readonly Storage _storage;
        private readonly List<ConfigSetting> _settings;
        private ReportingPeriod _defaultReportingPeriod;
        private Format _defaultReportFormat;
        private Format _defaultImportFormat;
        private DayOfWeek _startOfWeek;
        private decimal _roundingPrecision;

        public Configuration(Storage storage)
        {
            _storage = storage;

            string GetSetting(string name, string defaultValue = null)
            {
                var setting = _settings.FirstOrDefault(s => s.Key == name);
                if (setting == null)
                    _settings.Add(new ConfigSetting {Key = name, Value = defaultValue});
                return setting?.Value ?? defaultValue;
            }

            _settings = storage.ListConfigSettings().ToList();

            var value = GetSetting(DefaultReportingPeriodKey, "Yesterday");
            _defaultReportingPeriod = Enum.Parse<ReportingPeriod>(value, true);

            value = GetSetting(DefaultReportFormatKey, "Text");
            _defaultImportFormat = Enum.Parse<Format>(value, true);

            value = GetSetting(DefaultImportFormatKey, "Csv");
            _defaultImportFormat = Enum.Parse<Format>(value, true);

            value = GetSetting(StartOfWeekKey, "Monday");
            _startOfWeek = Enum.Parse<DayOfWeek>(value, true);

            value = GetSetting(RoundingKey, "0");
            _roundingPrecision = decimal.Parse(value);
        }

        public ReportingPeriod DefaultReportingPeriod
        {
            get => _defaultReportingPeriod;
            set
            {
                _defaultReportingPeriod = value;
                this[DefaultReportingPeriodKey] = value.ToString();
            }
        }

        public Format DefaultReportFormat
        {
            get => _defaultReportFormat;
            set
            {
                _defaultReportFormat = value;
                this[DefaultReportFormatKey] = value.ToString();
            }
        }

        public Format DefaultImportFormat
        {
            get => _defaultImportFormat;
            set
            {
                _defaultImportFormat = value;
                this[DefaultImportFormatKey] = value.ToString();
            }
        }

        public DayOfWeek StartOfWeek
        {
            get => _startOfWeek;
            set
            {
                _startOfWeek = value;
                this[StartOfWeekKey] = value.ToString();
            }
        }

        public decimal RoundingPrecision
        {
            get => _roundingPrecision;
            set
            {
                _roundingPrecision = value;
                this[RoundingKey] = value.ToString("F");
            }
        }

        public string this[string name]
        {
            get
            {
                var setting = _settings.SingleOrDefault(s => s.Key.EqualsIOC(name));
                if (setting == null)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(name),
                        $"'{name}' is not a known configuration setting");
                }

                return setting.Value;
            }
            set
            {
                var setting = _settings.SingleOrDefault(s => s.Key.EqualsIOC(name));
                if (setting == null)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(name),
                        $"'{name}' is not a known configuration setting");
                }

                setting.Value = value;
                _storage.Save(setting);
            }
        }

        public bool HasSetting(string setting)
        {
            return _settings.Any(s => s.Key.EqualsIOC(setting));
        }

        public IEnumerable<KeyValuePair<string, string>> Settings
        {
            get { return _settings.Select(s => new KeyValuePair<string, string>(s.Key, s.Value)); }
        }
    }
}