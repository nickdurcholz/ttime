using System;
using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbTimeEntry
{
    private TimeEntry _timeEntry;
    public LiteDbTimeEntry() : this(new ttime.TimeEntry()) { }
    public LiteDbTimeEntry(ttime.TimeEntry timeEntry)
    {
        _timeEntry = timeEntry;
    }

    public ObjectId Id { get; set; }

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

    [BsonIgnore]
    public ttime.TimeEntry Entry
    {
        get => _timeEntry;
        set => _timeEntry = value;
    }
}