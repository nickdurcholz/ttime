using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ttime;

public class EditCommand : Command
{
    public override void Run(Span<string> args)
    {
        DateTime? time = default;
        int? offset = default;
        DateTime? id = default;
        bool error = false;

        List<string> tags = new List<string>();

        foreach (var arg in args)
        {
            if (DateTime.TryParse(arg, out var t) || DateTimeUtility.TryParseDateOffset(arg, DateTime.Now, out t))
            {
                if (!id.HasValue)
                {
                    if (offset.HasValue)
                    {
                        Error.WriteLine($"You may not specify both a timestamp and an offset");
                        error = true;
                    }
                    id = t;
                }
                else if (time.HasValue)
                {
                    Error.WriteLine($"Duplicate time specified: {arg}");
                    error = true;
                }
                else
                    time = t;
            }
            else if (Regex.IsMatch(arg, @"^-\d+$"))
            {
                if (offset.HasValue)
                {
                    Error.WriteLine($"Duplicate offset specified: {arg}");
                    error = true;
                }
                else if (id != null)
                {
                    Error.WriteLine($"You may not specify both a timestamp and an offset");
                    error = true;
                }
                else
                {
                    offset = int.Parse(((ReadOnlySpan<char>) arg).Slice(1));
                }
            }
            else
                tags.Add(arg);
        }

        if (id == null && offset == null)
        {
            Error.WriteLine("You must provide either an offset or an timestamp");
            error = true;
        }

        if (error)
            return;

        var entry = id == null ? Storage.GetLastEntry(offset.Value) : Storage[id.Value];
        if (entry == null)
            throw new TTimeError("The indicated entry was not found");
        if (tags.Count > 0)
            entry.Tags = tags.ToArray();

        Storage.Save(entry, time);
    }

    public override void PrintUsage()
    {
        Out.WriteLine("usage: ttime edit -<offset>|<timestamp> [<date-time>] [<tag>...]");
        Out.WriteLine();
        Out.WriteLine("    Update an entry with a new time and/or tags. Entries may be specified by id");
        Out.WriteLine("    and/or an offset from the last one entered.");
        Out.WriteLine();
        Out.WriteLine("    You can specify by an entry using -offset. For example, -0 specifies the last");
        Out.WriteLine("    entry chronologically in the database, -1 is the second to last, and so on.");
        Out.WriteLine("    You may also provide the id of a specific record. Either an offset or an id");
        Out.WriteLine("    must be provided and the two are mutually exclusive.");
    }

    public override string Name => "edit";
    public override string Description => "edits an entry";
}