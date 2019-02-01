using System;

namespace ttime
{
    public class StopCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Out.WriteLine("Not implemented");
        }

        public override void PrintUsage()
        {
            Out.WriteLine("Not implemented");
        }

        public override string Name => "stop";
        public override string Description => "stops tracking time / 'clock-out'";
    }
}