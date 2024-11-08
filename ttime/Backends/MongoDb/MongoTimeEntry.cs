using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ttime.Backends.MongoDb;

public class MongoTimeEntry
{
    private TimeEntry _timeEntry;
    public MongoTimeEntry() : this(new TimeEntry()) { }

    public MongoTimeEntry(TimeEntry timeEntry)
    {
        _timeEntry = timeEntry;
    }

    public long _id
    {
        get => _timeEntry.Time.ToUnixTime();
        set => _timeEntry.Time = DateTimeUtility.UnixTimeToLocalTime(value);
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