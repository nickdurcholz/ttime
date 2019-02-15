using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

                    timeFound = DateTimeUtility.TryParseDateOffset(arg, DateTime.Now, out time);
                    if (timeFound) continue;
                }

                tags.Add(arg);
            }

            if (time == default)
                time = DateTime.Now;

            bool invalidTags = false;
            foreach (var tag in tags.Where(t => Enum.TryParse<ReportingPeriod>(t, true, out _)))
            {
                invalidTags = true;
                Error.WriteLine($"Invalid tag: {tag}. Cannot use the name of a reporting period.");
            }

            if (invalidTags)
                return;


            Storage.Save(new TimeEntry
            {
                Stopped = false,
                Tags = tags.ToArray(),
                Time = time,
            });
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime start [<date-time>|<offset>] [<tag>...]");
            Out.WriteLine();
            Out.WriteLine("    Starts tracking time optionally tagging it with one or more tasks that you");
            Out.WriteLine("    are working on. The start time is current date/time if omitted.");
            Out.WriteLine();
            Out.WriteLine("    Dates and times are always expressed as local time (not UTC), and can be");
            Out.WriteLine("    specified in a variety of formats recognized by .NET standard library. It");
            Out.WriteLine("    is usually convenient to use something like '2019-01-26T14:00'.");
            Out.WriteLine("    You can also supply an offset instead of an explicit time to");
            Out.WriteLine("    start the clock. Examples:");
            Out.WriteLine();
            Out.WriteLine("        ttime start -5m");
            Out.WriteLine("        ttime start 1.5h");
        }

        public override string Name => "start";
        public override string Description => "starts tracking time / 'clock-in'";
    }
}