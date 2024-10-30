using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ttime;

public class RemoveCommand : Command
{
    public override void Run(Span<string> args)
    {
        bool error = default;
        List<(int? offset, DateTime timestamp)> ids = new();

        foreach (var arg in args)
        {
            if (Regex.IsMatch(arg, @"^-\d+$"))
            {
                ids.Add((int.Parse(arg.Substring(1)), default));
            }
            else if (DateTime.TryParse(arg, out var dt))
            {
                ids.Add((null, dt));
            }
            else
            {
                Error.WriteLine($"Unrecognized argument: {arg}");
                error = true;
            }
        }

        if (error)
            return;

        var times = ids.Select(id => id.offset.HasValue ? Storage.GetLastEntry(id.offset.Value).Time : id.timestamp)
                       .ToList();
        Storage.DeleteEntries(times);
    }

    public override void PrintUsage()
    {
        Out.WriteLine("ttime rm -<offset>|<id> ...");
        Out.WriteLine();
        Out.WriteLine("    Delete an entry. Entries are specified as either an offset (e.g. -0 is the");
        Out.WriteLine("    last entry, -1 is the second-to-last, etc...) or a timestamp.");
    }

    public override string Name => "rm";
    public override string Description => "Delete an entry";
}