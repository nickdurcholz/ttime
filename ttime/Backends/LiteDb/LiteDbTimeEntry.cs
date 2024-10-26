using System;
using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbTimeEntry(TimeEntry timeEntry)
{
    public LiteDbTimeEntry() : this(new TimeEntry()) { }

    public ObjectId Id
    {
        get => string.IsNullOrEmpty(timeEntry.Id) ? null : new ObjectId(timeEntry.Id);
        set => timeEntry.Id = value.ToString();
    }

    public DateTime Time
    {
        get => timeEntry.Time;
        set => timeEntry.Time = value;
    }

    public bool Stopped
    {
        get => timeEntry.Stopped;
        set => timeEntry.Stopped = value;
    }

    public string[] Tags
    {
        get => timeEntry.Tags;
        set => timeEntry.Tags = value;
    }

    public TimeEntry Entry => timeEntry;
}