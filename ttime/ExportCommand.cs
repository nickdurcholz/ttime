using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ttime;

public class ExportCommand : Command
{
    public override void Run(Span<string> args)
    {
        ReportingPeriod period = default;
        OutputFormat format = default;
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
            if (Enum.TryParse<ReportingPeriod>(arg, true, out var p))
            {
                if (periodFound)
                {
                    Error.WriteLine("Multiple date ranges were specified. Please specify a single date or date range.");
                    valid = false;
                    continue;
                }

                period = p;
                periodFound = true;
                continue;
            }

            if (arg.StartsWith("from="))
            {
                if (periodFound && toDate != default)
                {
                    Error.WriteLine("Multiple date ranges were specified. Please specify a single date or date range.");
                    valid = false;
                    continue;
                }

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
                if (periodFound && toDate != default)
                {
                    Error.WriteLine("Multiple date ranges were specified. Please specify a single date or date range.");
                    valid = false;
                    continue;
                }

                if (arg.Length == 5)
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
            else if (DateTime.TryParse(arg, out var date))
            {
                if (periodFound)
                {
                    Error.WriteLine("Multiple date ranges were specified. Please specify a single date or date range.");
                    valid = false;
                    continue;
                }

                fromDate = date;
                toDate = fromDate.AddDays(1);
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
            format = Configuration.DefaultExportFormat;
        if (toDate == default)
            toDate = DateTime.Now;

        var formatter = Formatter.Create(format, Configuration.TimeFormat);

        var (start, end) = DateTimeUtility.ExpandPeriod(period, Configuration.StartOfWeek, fromDate, toDate);
        var entries = Storage.ListTimeEntries(start, end).ToList();
        if (tags.Count > 0)
        {
            entries.RemoveAll(e => e.Tags.All(et => tags.All(t => !Regex.IsMatch(et, t))));
        }

        TextWriter reportOut = Out;
        if (outFile != null)
            reportOut = new StreamWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None));

        try
        {
            formatter.Write(entries, reportOut);
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
        Out.WriteLine("usage: ttime export [day-of-week | last-week | yesterday | today |");
        Out.WriteLine("                    date | week | all | from=<date-time>");
        Out.WriteLine("                    [to=<date-time>]] [format=text|csv|xml|json]");
        Out.WriteLine("                    [out=<file>] [<tag>...]");
        Out.WriteLine();
        Out.WriteLine("    Export tracked time entries for the given period. These can be");
        Out.WriteLine("    edited and then imported.");
    }

    public override string Name => "export";
    public override string Description => "Export time data to a file";
}