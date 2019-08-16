using System;
using System.Collections.Generic;
using System.IO;

namespace ttime
{
    public class ReportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            ReportingPeriod period = default;
            Format format = default;
            string outFile = null;
            DateTime fromDate = default;
            DateTime toDate = default;
            ReportType reportType = default;

            var periodFound = false;
            var formatFound = false;
            var typeFound = false;
            var tags = new List<string>();
            var valid = true;
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (!periodFound)
                {
                    periodFound = Enum.TryParse(arg, true, out period);
                    if (periodFound) continue;

                    periodFound = DateTime.TryParse(arg, out fromDate);
                    if (periodFound)
                    {
                        toDate = fromDate.Date.AddDays(1);
                        period = ReportingPeriod.Custom;
                        continue;
                    }
                }

                if (arg.StartsWith("from="))
                {
                    if (arg.Length == 5)
                    {
                        Error.WriteLine("Invalid from date: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!DateTime.TryParse(arg.Substring(5), out fromDate))
                    {
                        Error.WriteLine("Invalid from date: " + arg);
                        valid = false;
                        continue;
                    }

                    periodFound = true;
                    period = ReportingPeriod.Custom;
                }
                else if (arg.StartsWith("to="))
                {
                    if (arg.Length == 3)
                    {
                        Error.WriteLine("Invalid to date: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!DateTime.TryParse(arg.Substring(3), out toDate))
                    {
                        Error.WriteLine("Invalid to date: " + arg);
                        valid = false;
                        continue;
                    }

                    periodFound = true;
                    period = ReportingPeriod.Custom;
                }
                else if (arg.StartsWith("format="))
                {
                    if (formatFound)
                    {
                        Error.WriteLine("Duplicate format specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 7)
                    {
                        Error.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!Enum.TryParse(arg.Substring(7), true, out format))
                    {
                        Error.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    formatFound = true;
                }
                else if (arg.StartsWith("type="))
                {
                    if (typeFound)
                    {
                        Error.WriteLine("Duplicate type specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 5)
                    {
                        Error.WriteLine("Invalid type: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!Enum.TryParse(arg.Substring(5), true, out reportType))
                    {
                        Error.WriteLine("Invalid type: " + arg);
                        valid = false;
                        continue;
                    }

                    typeFound = true;
                }
                else if (arg.StartsWith("out="))
                {
                    if (outFile != null)
                    {
                        Error.WriteLine("Duplicate output specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 4)
                    {
                        Error.WriteLine("Invalid output specification: " + arg);
                        valid = false;
                        continue;
                    }

                    outFile = arg.Substring(4);
                }
                else
                {
                    tags.Add(arg);
                }
            }

            if (!valid)
                return;

            if (!periodFound)
                period = Configuration.DefaultReportingPeriod;
            if (!formatFound)
                format = Configuration.DefaultReportFormat;
            if (!typeFound)
                reportType = Configuration.DefaultReportType;
            if (toDate == default)
                toDate = DateTime.Now;

            var calculator = new ReportCalculator(
                Storage,
                period,
                fromDate,
                toDate,
                tags,
                Configuration.StartOfWeek,
                Configuration.RoundingPrecision,
                reportType);
            var formatter = Formatter.Create(format);

            var reports = calculator.CreateReport();

            TextWriter reportOut = Out;
            if (outFile != null)
            {
                reportOut = new StreamWriter(new FileStream(outFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None));
            }

            try
            {
                formatter.Write(reports, reportOut);
            }
            finally
            {
                if (outFile != null)
                {
                    reportOut.Flush();
                    reportOut.Close();
                    reportOut.Dispose();
                }
            }
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime report [<day-of-week> | lastWeek | yesterday | today |");
            Out.WriteLine("                    <date> | week | all | from=<date-time>");
            Out.WriteLine("                    [to=<date-time>]] [format=text|csv|xml|json]");
            Out.WriteLine("                    [type=full|firstTag|daily] [out=<file>] [tag]...");
            Out.WriteLine();
            Out.WriteLine("    Print a report of how time was spent for a given period. When");
            Out.WriteLine("    invoked without specifying a period, the default period specified");
            Out.WriteLine("    in Configuration settings is used.");
            Out.WriteLine();
            Out.WriteLine("    A report type of full reports time on all tracked tags. If you started");
            Out.WriteLine("    tracking time at 9am with the tags task1 and task2 and stopped tracking");
            Out.WriteLine("    time at noon, then a full report would report that you worked for 3 hours");
            Out.WriteLine("    on task1 and 3 hours on task2 and worked for a total of 3 hours. While time");
            Out.WriteLine("    spent on individual tasks won't add up to the total if you were tracking");
            Out.WriteLine("    time on more than one task at a time, this is desirable for some reporting");
            Out.WriteLine("    scenarios.");
            Out.WriteLine();
            Out.WriteLine("    A report type of firstTag reports time on the first tracked tag for every");
            Out.WriteLine("    entry. If you started tracking time at 9am with the tags task1 and task2");
            Out.WriteLine("    and stopped tracking time at noon, a firstTag report would report that you");
            Out.WriteLine("    worked 3 hours on task1 and worked for a total of 3 hours. The benefit here");
            Out.WriteLine("    is that line items in the report will always add up to the total. You can");
            Out.WriteLine("    still get a report of how much time you spent on task2 by explicitly");
            Out.WriteLine("    requesting that tag.");
            Out.WriteLine();
            Out.WriteLine("    You can specify the default report type using 'ttime config'");
        }

        public override string Name => "report";
        public override string Description => "Print a report of how you spent your time";
    }
}