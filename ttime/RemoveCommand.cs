using System;
using System.Text.RegularExpressions;
using LiteDB;

namespace ttime;

public class RemoveCommand : Command
{
    public override void Run(Span<string> args)
    {
        int? offset = default;
        bool error = default;
        string id = default;

        foreach (var arg in args)
        {
            if (Regex.IsMatch(arg, @"^-\d+$"))
            {
                if (offset.HasValue)
                {
                    Error.WriteLine($"Duplicate offset specified: {arg}");
                    error = true;
                }
                else if (id != null)
                {
                    Error.WriteLine($"You may not specify both an id and an offset");
                    error = true;
                }
                else
                {
                    offset = int.Parse(((ReadOnlySpan<char>) arg).Slice(1));
                }
            }
            else if (Regex.IsMatch(arg, "^[0-9a-fA-F]{24}$"))
            {
                if (offset.HasValue)
                {
                    Error.WriteLine($"You may not specify both an id and an offset");
                    error = true;
                }
                else if (id != null)
                {
                    Error.WriteLine($"Duplicate id specified: {arg}");
                    error = true;
                }
                else
                {
                    id = arg;
                }
            }
            else
            {
                Error.WriteLine($"Unrecognized argument: {arg}");
                error = true;
            }
        }

        if (error)
            return;

        var oid = offset.HasValue ? Storage.GetLastEntry(offset.Value).Id : new ObjectId(id);
        Storage.DeleteEntry(oid);
    }

    public override void PrintUsage()
    {
        Out.WriteLine("ttime rm [-<offset>|<id>]");
        Out.WriteLine();
        Out.WriteLine("    Delete an entry. Entries are specified as either an offset (e.g. -0 is the");
        Out.WriteLine("    last entry, -1 is the second-to-last, etc...) or an id.");

    }

    public override string Name => "rm";
    public override string Description => "Delete an entry";
}