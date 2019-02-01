using System;

namespace ttime
{
    public class ImportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            throw new NotImplementedException();
        }

        public override void PrintUsage()
        {
            throw new NotImplementedException();
        }

        public override string Name => "import";
        public override string Description => "Import time data from a file";
    }
}