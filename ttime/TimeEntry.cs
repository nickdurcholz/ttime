using System;
using LiteDB;

namespace ttime
{
    public class TimeEntry
    {
        public ObjectId Id { get; set; }
        public DateTime Time { get; set; }
        public bool Stopped { get; set; }
        public string[] Tags { get; set; }
    }
}