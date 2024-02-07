using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LiteDB;

namespace ttime;

public class RemoveCommand : Command
{
    public override void Run(Span<string> args)
    {
        bool error = default;
        List<(bool isId, string value)> ids = new();

        foreach (var arg in args)
        {
            if (Regex.IsMatch(arg, @"^-\d+$"))
            {
                ids.Add((false, arg.Substring(1)));
            }
            else if (Regex.IsMatch(arg, "^[0-9a-fA-F]{24}$"))
            {
                ids.Add((true, arg));
            }
            else
            {
                Error.WriteLine($"Unrecognized argument: {arg}");
                error = true;
            }
        }

        if (error)
            return;

        foreach (var id in ids)
        {
            var oid = id.isId ? new ObjectId(id.value) : Storage.GetLastEntry(int.Parse(id.value)).Id;
            Storage.DeleteEntry(oid);
        }
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