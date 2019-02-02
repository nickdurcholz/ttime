using System;
using System.Text.RegularExpressions;

namespace ttime
{
    public class HelpCommand : Command
    {
        private static readonly Regex descriptionSplitter = new Regex(@"\S+", RegexOptions.Compiled);

        public override void Run(Span<string> args)
        {
            var subtopic = args.Length > 0 ? args[0] : null;
            var topicCommand = Program.GetCommand(subtopic) ?? this;

            topicCommand.PrintUsage();
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime <command> [<args>]");
            Out.WriteLine();
            Out.WriteLine("Available commands:");

            var descriptionStart = 0;
            foreach (var command in Program.AvailableCommands)
            {
                if (descriptionStart < command.Name.Length + 5)
                    descriptionStart = command.Name.Length + 5;
            }

            descriptionStart = ((descriptionStart / 4) + 1) * 4;

            foreach (var command in Program.AvailableCommands)
            {
                Out.Write("    ");
                Out.WriteAndPad(command.Name, descriptionStart);

                var descriptionTokens = descriptionSplitter.Matches(command.Description);
                int currentLineLength = descriptionStart;
                for (var i = 0; i < descriptionTokens.Count; i++)
                {
                    Match token = descriptionTokens[i];
                    Out.Write(token.Value);
                    currentLineLength += token.Value.Length;

                    if (i == descriptionTokens.Count - 1 ||
                        currentLineLength + descriptionTokens[i + 1].Value.Length > 80)
                    {
                        Out.WriteLine();
                        if (i < descriptionTokens.Count - 1)
                        {
                            for (var j = 0; j < descriptionStart; j++)
                                Out.Write(' ');
                            currentLineLength = descriptionStart;
                        }
                    }
                    else
                    {
                        Out.Write(' ');
                        currentLineLength++;
                    }
                }
           }
        }

        public override string Name => "help";

        public override string Description => "Shows this message. " +
                                              "Can also show specific help for another command using 'ttime help <command>'.";
    }
}