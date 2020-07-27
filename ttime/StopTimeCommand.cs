using System;
using System.Collections.Generic;
using System.Linq;

namespace ttime
{
    public class StopTimeCommand : Command
    {
        public override void Run(Span<string> args)
        {
            decimal hours = 0;
            var considerWeeklyHours = true;
            if (args.Length == 1)
            {
                if (!decimal.TryParse(args[0], out hours))
                {
                    Error.WriteLine("Invalid number of hours: " + args[0]);
                    return;
                }

                considerWeeklyHours = false;
            }
            else if (args.Length > 1)
            {
                Error.WriteLine("Invalid arguments. Expected zero or one arguments but got " + args.Length);
                PrintUsage();
                return;
            }

            hours = GetStopTime(hours, ReportingPeriod.Today);
            if (considerWeeklyHours)
                hours = Math.Min(hours, GetStopTime(Configuration.HoursPerWeek, ReportingPeriod.Week));

            Out.WriteLine(DateTime.Now.AddMilliseconds((double) hours * 3600000));
        }

        private decimal GetStopTime(decimal hours, ReportingPeriod reportingPeriod)
        {
            if (hours <= 0)
                hours = 8m;

            var calculator = new ReportCalculator(
                Storage,
                reportingPeriod,
                default,
                default,
                Configuration.StartOfWeek,
                Configuration.RoundingPrecision);

            var report = calculator.CreateReport();

            hours -= report.Single().Hours;
            return hours;
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime stop-time [<hours>]");
            Out.WriteLine();
            Out.WriteLine("    Show what time you can stop working today and have worked a full");
            Out.WriteLine("    day or week, whichever is earlier.");
            Out.WriteLine();
            Out.WriteLine("    Optionally specify a number of hours (e.g. '8.5') that you want");
            Out.WriteLine("    to work. Defaults to 8 hours if omitted.");
        }

        public override string Name => "stop-time";
        public override string Description => "Shows what time you can stop working and still put in a full day";
    }
}