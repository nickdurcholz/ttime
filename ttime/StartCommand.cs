using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace ttime
{
    public class StartCommand : Command
    {
        private readonly LiteDatabase _db;

        public StartCommand(LiteDatabase db)
        {
            _db = db;
        }

        public override void Run(Span<string> args)
        {
            DateTime time = default;
            var tags = new List<string>();
            bool timeFound = false;

            foreach (var arg in args)
            {
                if (!timeFound)
                {
                    timeFound = DateTime.TryParse(arg, out time);
                    if (timeFound) continue;
                }

                tags.Add(arg);
            }

            if (tags.Count == 0)
                tags.Add("Unspecified");

            if (time == default)
                time = DateTime.Now;

            var collection = _db.GetCollection<TimeEntry>("log");
            collection.EnsureIndex(e => e.Time);
            collection.Insert(new TimeEntry
            {
                Stopped = false,
                Tags = tags.ToArray(),
                Time = time,
            });
        }

        public override void PrintUsage(TextWriter @out)
        {
            @out.WriteLine("usage: ttime start [date-time] [<tag1> ... <tagN>]");
            @out.WriteLine();
            @out.WriteLine("    Starts tracking time optionally tagging it with one or more tasks that you");
            @out.WriteLine("    are working on. The start time is current date/time if omitted.");
            @out.WriteLine("    ");
            @out.WriteLine("    Dates and times are always expressed as local time (not UTC), and can be");
            @out.WriteLine("    specified in a variety of formats recognized by .NET standard library. It");
            @out.WriteLine("    is usually convenient to use something like '2019-01-26T14:00'.");
        }
    }
}