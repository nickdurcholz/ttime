using System;

namespace ttime.Backends.MongoDb;

public class MongoTimeEntry
{
    private TimeEntry _timeEntry;
    public MongoTimeEntry() : this(new TimeEntry()) { }

    public MongoTimeEntry(TimeEntry timeEntry)
    {
        _timeEntry = timeEntry;
    }

    public long _id => Time.ToUnixTime();

    public DateTime Time
    {
        get => _timeEntry.Time;
        set => _timeEntry.Time = value;
    }

    public bool Stopped
    {
        get => _timeEntry.Stopped;
        set => _timeEntry.Stopped = value;
    }

    public string[] Tags
    {
        get => _timeEntry.Tags;
        set => _timeEntry.Tags = value;
    }

    public ttime.TimeEntry Entry
    {
        get => _timeEntry;
        set => _timeEntry = value;
    }
}