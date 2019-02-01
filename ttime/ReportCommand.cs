using System;

namespace ttime
{
    public class ReportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Out.WriteLine("Not implemented");
        }

        public override void PrintUsage()
        {
            Out.WriteLine("Not implemented");
        }

        public override string Name => "report";
        public override string Description => "Print a report of how you spent your time";
    }
}