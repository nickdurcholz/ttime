using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbConfigSetting(ConfigSetting setting)
{
    public LiteDbConfigSetting() : this(new ConfigSetting()) { }

    public ObjectId Id
    {
        get => string.IsNullOrEmpty(setting.Id) ? null : new(setting.Id);
        set => setting.Id = value.ToString();
    }

    public string Key
    {
        get => setting.Key;
        set => setting.Key = value;
    }

    public string Value
    {
        get => setting.Value;
        set => setting.Value = value;
    }

    public ConfigSetting Setting => setting;
}