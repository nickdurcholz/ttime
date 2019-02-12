using System;
using System.Collections.Generic;

namespace ttime
{
    public class StopTimeCommand : Command
    {
        public override void Run(Span<string> args)
        {
            decimal hours = 0;
            if (args.Length == 1)
            {
                if (!decimal.TryParse(args[0], out hours))
                {
                    Error.WriteLine("Invalid number of hours: " + args[0]);
                    return;
                }
            }
            else if (args.Length > 1)
            {
                Error.WriteLine("Invalid arguments. Expected zero or one arguments but got " + args.Length);
                PrintUsage();
                return;
            }

            if (hours <= 0)
                hours = 8m;

            var calculator = new ReportCalculator(
                Storage,
                ReportingPeriod.Today,
                default,
                default,
                new List<string>(),
                Configuration.StartOfWeek,
                Configuration.RoundingPrecision,
                default);

            var report = calculator.CreateReport();

            hours -= report.Total;

            Out.WriteLine(DateTime.Now.AddMilliseconds((double)hours * 3600000));
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime stop-time [<hours>]");
            Out.WriteLine();
            Out.WriteLine("    Show what time you can stop working today and have worked the");
            Out.WriteLine("    given amount of time.");
            Out.WriteLine();
            Out.WriteLine("    Optionally specify a number of hours (e.g. '8.5') that you want");
            Out.WriteLine("    to work. Defaults to 8 hours if omitted.");
        }

        public override string Name => "stop-time";
        public override string Description => "Shows what time you can stop working and still put in a full day";
    }
}