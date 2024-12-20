using MongoDB.Bson.Serialization.Attributes;

namespace ttime.Backends.MongoDb;

public class MongoConfigSetting
{
    private ConfigSetting _setting;
    public MongoConfigSetting() : this(new ConfigSetting()) { }

    public MongoConfigSetting(ConfigSetting setting)
    {
        _setting = setting;
    }

    public string _id
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