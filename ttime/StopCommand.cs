using System;
using System.Collections.Generic;

namespace ttime
{
    public class StopCommand : Command
    {
        public override void Run(Span<string> args)
        {
            DateTime time = default;
            bool timeFound = false;

            foreach (var arg in args)
            {
                if (!timeFound)
                {
                    timeFound = DateTime.TryParse(arg, out time);
                    if (!timeFound)
                        Error.WriteLine($"Unrecognized argument: {arg}");
                }
                else
                    Error.WriteLine($"Unrecognized argument: {arg}");
            }

            if (time == default)
                time = DateTime.Now;

            Storage.Save(new TimeEntry
            {
                Stopped = true,
                Tags = new string[0],
                Time = time,
            });
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime stop [date-time]");
            Out.WriteLine();
            Out.WriteLine("    Stops tracking time. Use 'start' to resume tracking.");
            Out.WriteLine();
            Out.WriteLine("    Dates and times are always input as local time and stored as UTC.");
            Out.WriteLine("    They can be specified in a variety of formats recognized by .NET");
            Out.WriteLine("    standard library. It is usually convenient to use something like");
            Out.WriteLine("    '2019-01-26T14:00'.");
        }

        public override string Name => "stop";
        public override string Description => "stops tracking time / 'clock-out'";
    }
}