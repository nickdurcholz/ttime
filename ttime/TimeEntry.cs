using System;
using LiteDB;
using Newtonsoft.Json;

namespace ttime
{
    public class TimeEntry
    {
        [JsonConverter(typeof(JsonObjectIdConverter))]
        public ObjectId Id { get; set; }

        public DateTime Time { get; set; }
        public bool Stopped { get; set; }
        public string[] Tags { get; set; }
    }
}