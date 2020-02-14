using System;
using System.Collections.Generic;
using System.Linq;

namespace ttime
{
    public class Configuration
    {
        private const string DefaultReportingPeriodKey = "defaultReportPeriod";
        private const string DefaultReportFormatKey = "defaultReportFormat";
        private const string DefaultReportTypeKey = "defaultReportType";
        private const string DefaultExportFormatKey = "defaultExportFormat";
        private const string StartOfWeekKey = "startOfWeek";
        private const string RoundingKey = "rounding";
        private const string HoursPerWeekKey = "hoursPerWeek";

        private readonly Storage _storage;
        private readonly List<ConfigSetting> _settings;
        private ReportingPeriod _defaultReportingPeriod;
        private Format _defaultReportFormat;
        private Format _defaultExportFormat;
        private DayOfWeek _startOfWeek;
        private decimal _roundingPrecision;
        private ReportType _defaultReportType;
        private int _hoursPerWeek;

        public Configuration(Storage storage)
        {
            _storage = storage;

            string GetSetting(string name, string defaultValue = null)
            {
                var setting = _settings.FirstOrDefault(s => s.Key == name);
                if (setting == null)
                    _settings.Add(new ConfigSetting { Key = name, Value = defaultValue });
                return setting?.Value ?? defaultValue;
            }

            _settings = storage.ListConfigSettings().ToList();

            var value = GetSetting(DefaultReportingPeriodKey, nameof(ReportingPeriod.Yesterday));
            _defaultReportingPeriod = Enum.Parse<ReportingPeriod>(value, true);

            value = GetSetting(DefaultReportFormatKey, nameof(Format.Text));
            _defaultReportFormat = Enum.Parse<Format>(value, true);

            value = GetSetting(DefaultReportTypeKey, nameof(ReportType.FirstTag));
            _defaultReportType = Enum.Parse<ReportType>(value, true);

            value = GetSetting(DefaultExportFormatKey, nameof(Format.Csv));
            _defaultExportFormat = Enum.Parse<Format>(value, true);

            value = GetSetting(StartOfWeekKey, nameof(DayOfWeek.Monday));
            _startOfWeek = Enum.Parse<DayOfWeek>(value, true);

            value = GetSetting(RoundingKey, "0");
            _roundingPrecision = decimal.Parse(value);

            if (!int.TryParse(GetSetting(HoursPerWeekKey), out _hoursPerWeek))
                _hoursPerWeek = 40;

            Aliases = storage.ListAliases().ToList();
        }

        public List<Alias> Aliases { get; set; }

        public int HoursPerWeek
        {
            get => _hoursPerWeek;
            set
            {
                _hoursPerWeek = value;
                this[HoursPerWeekKey] = value.ToString();
            }
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

        public ReportType DefaultReportType
        {
            get => _defaultReportType;
            set
            {
                _defaultReportType = value;
                this[DefaultReportTypeKey] = value.ToString();
            }
        }

        public Format DefaultExportFormat
        {
            get => _defaultExportFormat;
            set
            {
                _defaultExportFormat = value;
                this[DefaultExportFormatKey] = value.ToString();
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
                        $"'{name}' is not a known Configuration setting");
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
                        $"'{name}' is not a known Configuration setting");
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

        public void SetAlias(string name, List<string> arguments)
        {
            var alias = Aliases.FirstOrDefault(a => a.Name.EqualsIOC(name));
            if (alias == null)
            {
                alias = new Alias();
                Aliases.Add(alias);
            }

            alias.Name = name;
            alias.Args = arguments;
            _storage.Save(alias);
        }

        public void DeleteAlias(string name)
        {
            var alias = Aliases.FirstOrDefault(a => a.Name.EqualsIOC(name));
            if (alias != null)
            {
                Aliases.Remove(alias);
                _storage.Delete(alias);
            }
        }
    }
}