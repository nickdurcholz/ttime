using System;

namespace ttime
{
    public class ImportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Error.WriteLine("Not implemented");
        }

        public override void PrintUsage()
        {
            Error.WriteLine("Not implemented");
        }

        public override string Name => "import";
        public override string Description => "Import time data from a file";
    }
}