using System;

namespace ttime
{
    public class TimeEntry
    {
        public DateTime Time { get; set; }
        public bool Stopped { get; set; }
        public string[] Tags { get; set; }
    }
}