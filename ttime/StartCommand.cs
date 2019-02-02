using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;

namespace ttime
{
    public class StartCommand : Command
    {
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

            if (time == default)
                time = DateTime.Now;

            Storage.Save(new TimeEntry
            {
                Stopped = false,
                Tags = tags.ToArray(),
                Time = time,
            });
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime start [<date-time>] [<tag>...]");
            Out.WriteLine();
            Out.WriteLine("    Starts tracking time optionally tagging it with one or more tasks that you");
            Out.WriteLine("    are working on. The start time is current date/time if omitted.");
            Out.WriteLine();
            Out.WriteLine("    Dates and times are always expressed as local time (not UTC), and can be");
            Out.WriteLine("    specified in a variety of formats recognized by .NET standard library. It");
            Out.WriteLine("    is usually convenient to use something like '2019-01-26T14:00'.");
        }

        public override string Name => "start";
        public override string Description => "starts tracking time / 'clock-in'";
    }
}