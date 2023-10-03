using System;
using System.Diagnostics;
using LiteDB;
using Newtonsoft.Json;

namespace ttime;

[DebuggerDisplay("{DebuggerDisplay}")]
public class TimeEntry
{
    [JsonConverter(typeof(JsonObjectIdConverter))]
    public ObjectId Id { get; set; }

    public DateTime Time { get; set; }
    public bool Stopped { get; set; }
    public string[] Tags { get; set; }

    private string DebuggerDisplay => Stopped
        ? $"Stopped {Time:s}"
        : $"{(Tags == null || Tags.Length == 0 ? "Unspecfied" : string.Join(' ', Tags))} {Time:s}";
}