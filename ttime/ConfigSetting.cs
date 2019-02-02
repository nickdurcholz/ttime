using LiteDB;

namespace ttime
{
    public class ConfigSetting
    {
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}