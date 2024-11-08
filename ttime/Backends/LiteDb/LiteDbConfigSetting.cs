using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbConfigSetting
{
    private ConfigSetting _setting;
    public LiteDbConfigSetting() : this(new ConfigSetting()) { }
    public LiteDbConfigSetting(ConfigSetting setting)
    {
        _setting = setting;
    }

    public ObjectId Id { get; set; }

    public string Key
    {
        get => _setting.Key;
        set => _setting.Key = value;
    }

    public string Value
    {
        get => _setting.Value;
        set => _setting.Value = value;
    }

    [BsonIgnore]
    public ConfigSetting Setting
    {
        get => _setting;
        set => _setting = value;
    }
}