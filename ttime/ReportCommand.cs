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


            var periodFound = false;
            var formatFound = false;
            var tags = new List<string>();
            var valid = true;
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (!periodFound)
                {
                    periodFound = Enum.TryParse(arg, out period);
                    if (periodFound) continue;
                }

                if (arg.StartsWith("from="))
                {
                    if (arg.Length == 5)
                    {
                        Out.WriteLine("Invalid from date: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!DateTime.TryParse(arg.Substring(5), out fromDate))
                    {
                        Out.WriteLine("Invalid from date: " + arg);
                        valid = false;
                        continue;
                    }

                    periodFound = true;
                    period = ReportingPeriod.Custom;
                }

                if (arg.StartsWith("to="))
                {
                    if (arg.Length == 5)
                    {
                        Out.WriteLine("Invalid to date: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!DateTime.TryParse(arg.Substring(5), out toDate))
                    {
                        Out.WriteLine("Invalid to date: " + arg);
                        valid = false;
                        continue;
                    }
                    periodFound = true;
                    period = ReportingPeriod.Custom;
                }

                if (arg.StartsWith("format="))
                {
                    if (formatFound)
                    {
                        Out.WriteLine("Duplicate format specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 7)
                    {
                        Out.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!Enum.TryParse(arg.Substring(7), out format))
                    {
                        Out.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    formatFound = true;
                }

                if (arg.StartsWith("out="))
                {
                    if (outFile != null)
                    {
                        Out.WriteLine("Duplicate output specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 4)
                    {
                        Out.WriteLine("Invalid output specification: " + arg);
                        valid = false;
                        continue;
                    }

                    outFile = arg.Substring(4);
                }

                tags.Add(arg);
            }

            if (!valid)
                return;

            Configuration config = new Configuration(Db);
            if (!periodFound)
                period = config.DefaultReportingPeriod;
            if (!formatFound)
                format = config.DefaultFormat;

            var calculator = new ReportCalculator(Db, period, fromDate, toDate, tags);
            var formatter = Formatter.Create(format);

            var report = calculator.CreateReport();

            TextWriter reportOut = Out;
            if (outFile != null)
                reportOut = new StreamWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None));

            try
            {
                formatter.Write(report, reportOut);
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
            Out.WriteLine("usage: ttime report [day-of-week | last-week | yesterday | today |");
            Out.WriteLine("                    date | from=date-time [to=date-time]]");
            Out.WriteLine("                    [format=text|csv|xml|json] [tag] [out=<file>]");
            Out.WriteLine();
            Out.WriteLine("    Print a report of how time was spent for a given period. When");
            Out.WriteLine("    invoked without specifying a period, the default period specified");
            Out.WriteLine("    in configuration settings is used.");
        }

        public override string Name => "report";
        public override string Description => "Print a report of how you spent your time";
    }
}