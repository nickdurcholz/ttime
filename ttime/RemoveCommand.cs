using System;
using LiteDB;

namespace ttime
{
    public class RemoveCommand : Command
    {
        public override void Run(Span<string> args)
        {
            if (args.Length == 0)
                Error.WriteLine("Id is required.");
            else if (args.Length > 1)
                Error.WriteLine("Unexpected arguments: " + string.Join(" ", args.Slice(1).ToArray()));
            else
            {
                Storage.DeleteEntry(new ObjectId(args[0]));
            }
        }

        public override void PrintUsage()
        {
            Out.WriteLine("ttime rm <id>");
            Out.WriteLine();
            Out.WriteLine("    Delete an entry by id");
        }

        public override string Name => "rm";
        public override string Description => "Delete an entry";
    }
}