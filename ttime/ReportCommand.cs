using System;
using System.Collections.Generic;
using System.IO;
using ttime.Formatters;

namespace ttime;

public class ReportCommand : Command
{
    public override void Run(Span<string> args)
    {
        ReportingPeriod period = default;
        ReportFormat dataFormat = default;
        string outFile = null;
        DateTime fromDate = default;
        DateTime toDate = default;
        var daily = false;
        int? nestingLevel = null;
        decimal roundingPrecision = Configuration.RoundingPrecision;
        bool roundingPrecisionFound = false;
        TimeFormat timeFormat = Configuration.TimeFormat;
        bool timeFormatFound = false;

        var periodFound = false;
        var outputFormatFound = false;
        var tags = new List<string>();
        var valid = true;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (int.TryParse(arg, out var year) && year > 0)
            {
                if (periodFound)
                {
                    Error.WriteLine($"Invalid reporting period {year}. Please specify a single date or date range.");
                    valid = false;
                    continue;
                }

                period = ReportingPeriod.Custom;
                periodFound = true;

                fromDate = new DateTime(year, 1, 1);
                toDate = new DateTime(year + 1, 1, 1);
            }
            else if (!periodFound)
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
                if (outputFormatFound)
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

                if (!Enum.TryParse(arg.Substring(7), true, out dataFormat))
                {
                    Error.WriteLine("Invalid format: " + arg);
                    valid = false;
                    continue;
                }

                outputFormatFound = true;
            }
            else if (arg.StartsWith("disp="))
            {
                if (timeFormatFound)
                {
                    Error.WriteLine("Duplicate time format specification found: " + arg);
                    valid = false;
                    continue;
                }

                if (arg.Length == 5)
                {
                    Error.WriteLine("Invalid time format: " + arg);
                    valid = false;
                    continue;
                }

                if (!Enum.TryParse(arg.Substring(5), true, out timeFormat))
                {
                    Error.WriteLine("Invalid time format: " + arg);
                    valid = false;
                    continue;
                }

                timeFormatFound = true;
            }
            else if (arg.StartsWith("round="))
            {
                if (roundingPrecisionFound)
                {
                    Error.WriteLine("Duplicate rounding specification found: " + arg);
                    valid = false;
                    continue;
                }

                if (arg.Length == 6)
                {
                    Error.WriteLine("Invalid rounding factor: " + arg);
                    valid = false;
                    continue;
                }

                if (!decimal.TryParse(arg.Substring(6), out roundingPrecision))
                {
                    Error.WriteLine("Invalid rounding factor: " + arg);
                    valid = false;
                    continue;
                }

                roundingPrecisionFound = true;
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
        if (!outputFormatFound)
            dataFormat = Configuration.DefaultReportFormat;
        if (toDate == default)
            toDate = DateTime.Now;

        var calculator = new ReportCalculator(
            Storage,
            period,
            fromDate,
            toDate,
            Configuration.StartOfWeek,
            roundingPrecision,
            daily,
            tags);
        var formatter = FormatterFactory.GetReportFormatter(dataFormat, timeFormat);

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
        Out.WriteLine("                    <date> | week | all | <year> | from=<date-time>");
        Out.WriteLine("                    [to=<date-time>]]");
        Out.WriteLine("                    [format=text|CsvSimple|CsvRollup|xml|json]");
        Out.WriteLine("                    [n=3] [daily=y|n] [out=<file>] [round=x] ");
        Out.WriteLine("                    [disp=DecimalHours|HoursMinutes] [tag]...");
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
        Out.WriteLine();
        Out.WriteLine("    When providing a tag list to report on, tag names are interpreted");
        Out.WriteLine("    as a regular expression. Hours that are tagged with a tag that ");
        Out.WriteLine("    matches any of the provided expressions are reported.");
    }

    public override string Name => "report";
    public override string Description => "Print a report of how you spent your time";
}