using System;
using System.Linq;

namespace ttime;

public class ConfigCommand : Command
{
    public override void Run(Span<string> args)
    {
        string setting = null;
        string value = null;

        if (args.Length > 0)
        {
            setting = args[0];
            if (!Configuration.HasSetting(setting))
            {
                Error.WriteLine("Unknown Configuration setting: " + setting);
                return;
            }
        }

        if (args.Length == 2)
            value = args[1];

        if (args.Length > 2)
        {
            for (int i = 2; i < args.Length; i++)
                Error.WriteLine("Unrecognized argument: " + args[i]);
            return;
        }

        if (!string.IsNullOrEmpty(setting) && !string.IsNullOrEmpty(value))
        {
            Configuration[setting] = value;
        }
        else if (!string.IsNullOrEmpty(setting))
        {
            Out.WriteLine(Configuration[setting]);
        }
        else
        {
            var ls = Configuration.Settings.ToList();
            var width = ((ls.Max(s => s.Key.Length) / 4) + 1) * 4;
            foreach (var kvp in ls)
            {
                Out.WriteAndPad(kvp.Key, width);
                Out.WriteLine(kvp.Value);
            }
        }
    }

    public override void PrintUsage()
    {
        Out.WriteLine("ttime config [<setting>] [<value>]");

        Out.WriteLine("    Show or modify ttime settings. Without arguments, this will print a list of");
        Out.WriteLine("    all settings and their current values. Specify a setting without a value to");
        Out.WriteLine("    print its current value. Specify both a setting and a value to change a");
        Out.WriteLine("    setting.");
        Out.WriteLine();
        Out.WriteLine("    Known Configuration settings");
        Out.WriteLine("    ----------------------------");
        Out.WriteLine();
        Out.WriteLine("    rounding            Controls rounding when generating reports.");
        Out.WriteLine("                        A value greater than zero indicates that totals should");
        Out.WriteLine("                        be rounded to the nearest number of hours. For example,");
        Out.WriteLine("                        specify 0.25 to round to the nearest quarter hour");
        Out.WriteLine("                        causing all totals to end with .0, .25, .5, or .25");
        Out.WriteLine();
        Out.WriteLine("    defaultReportPeriod The default period to use when a period is not provided");
        Out.WriteLine("                        to the report command");
        Out.WriteLine();
        Out.WriteLine("    defaultFormat       The default output format for reports.");
        Out.WriteLine("    startOfWeek         The start of the work week to use in calculations.");
        Out.WriteLine("                        Affects reporting for the lastWeek and week report periods.");
        Out.WriteLine();
        Out.WriteLine("    hoursPerWeek        The target number of hours you normally work in a week.");
    }

    public override string Name => "config";
    public override string Description => "Show / modify ttime settings";
}