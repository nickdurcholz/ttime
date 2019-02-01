using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ttime
{
    public class UsageCommand : Command
    {
        private readonly TextWriter _out;

        public UsageCommand(TextWriter @out)
        {
            _out = @out;
        }

        public override void Run(Span<string> args)
        {
            var subtopic = args.Length > 0 ? args[0] : null;
            var topicCommand = Program.GetCommand(subtopic) ?? this;

            topicCommand.PrintUsage(_out);
        }

        public override void PrintUsage(TextWriter @out)
        {
            @out.WriteLine("usage: ttime <command> [<args>]");
            @out.WriteLine();
            @out.WriteLine("Available commands:");
            @out.WriteLine("    help        Shows this message or specific help for another command using");
            @out.WriteLine("                'ttime help <command>'");
            @out.WriteLine("    start       Starts tracking time. \"Clock-in\"");
            @out.WriteLine("    stop        Stop tracking time. \"Clock-out\"");
            @out.WriteLine("    report      Print a report of how you spent your time");
            @out.WriteLine("    config      Show / modify ttime settings");
        }
    }
}