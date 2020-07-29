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
            var daily = false;
            int? nestingLevel = null;

            var periodFound = false;
            var formatFound = false;
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
                else if (arg.StartsWith("daily="))
                {
                    if (outFile != null)
                    {
                        Error.WriteLine("Duplicate output specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 6)
                    {
                        Error.WriteLine("Invalid output specification: " + arg);
                        valid = false;
                        continue;
                    }

                    var val = arg.Substring(6);
                    daily = string.Equals("y", val, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("yes", val, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("t", val, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("true", val, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("1", val, StringComparison.OrdinalIgnoreCase);
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
                else if (arg.StartsWith("n="))
                {
                    if (nestingLevel != null)
                    {
                        Error.WriteLine("Duplicate combine count arg: " + arg);
                        valid = false;
                        continue;
                    }

                    if (int.TryParse(arg.AsSpan(2), out var n))
                    {
                        nestingLevel = n;
                    }
                    else
                    {
                        Error.WriteLine("Invalid combine count : " + arg);
                        valid = false;
                        continue;
                    }
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
            if (toDate == default)
                toDate = DateTime.Now;

            var calculator = new ReportCalculator(
                Storage,
                period,
                fromDate,
                toDate,
                Configuration.StartOfWeek,
                Configuration.RoundingPrecision,
                daily,
                tags);
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
                formatter.Write(reports, reportOut, nestingLevel);
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
            Out.WriteLine("                    [n=3] [daily=y|n] [out=<file>] [tag]...");
            Out.WriteLine();
            Out.WriteLine("    Print a report of how time was spent for a given period. When");
            Out.WriteLine("    invoked without specifying a period, the default period specified");
            Out.WriteLine("    in Configuration settings is used.");
            Out.WriteLine();
            Out.WriteLine("    n is the 'nesting level', which controls heirarchical display of");
            Out.WriteLine("    reports when formatted as plain text.");
            Out.WriteLine();
            Out.WriteLine("    specify daily=y to display a separate report for each day");
            Out.WriteLine("    contained in the requested period.");
        }

        public override string Name => "report";
        public override string Description => "Print a report of how you spent your time";
    }
}