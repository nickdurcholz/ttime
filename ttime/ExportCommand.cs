using System;

namespace ttime
{
    public class ExportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Out.WriteLine("Not implemented");
        }

        public override void PrintUsage()
        {
            Out.WriteLine("Not implemented");
        }

        public override string Name => "export";
        public override string Description => "Export time data to a file";
    }
}