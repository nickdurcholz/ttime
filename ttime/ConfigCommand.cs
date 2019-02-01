using System;

namespace ttime
{
    public class ConfigCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Error.WriteLine("Not implemented");
        }

        public override void PrintUsage()
        {
            Error.WriteLine("Not implemented");
        }

        public override string Name => "config";
        public override string Description => "Show / modify ttime settings";
    }
}