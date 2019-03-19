using System;
using System.Collections.Generic;
using System.Linq;

namespace ttime
{
    public class AliasCommand : Command
    {
        public override void Run(Span<string> args)
        {
            string name = null;
            List<string> arguments = new List<string>();
            if (args.Length > 0)
                name = args[0];

            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                    arguments.Add(args[i]);
            }

            if (name == null)
            {
                if (Configuration.Aliases.Count == 0)
                    Out.WriteLine("No aliases are defined");
                else
                {
                    var width = (Configuration.Aliases.Max(a => a.Name.Length) / 4 + 1) * 4;
                    foreach (var alias in Configuration.Aliases)
                    {
                        Out.WriteAndPad(alias.Name, width);
                        foreach (var arg in alias.Args)
                        {
                            Out.Write(' ');
                            Out.Write(arg);
                        }

                        Out.WriteLine();
                    }
                }
            }
            else if (arguments.Count == 0)
            {
                var alias = Configuration.Aliases.FirstOrDefault(a => a.Name.EqualsIOC(name));
                if (alias != null)
                {
                    for (var i = 0; i < alias.Args.Count; i++)
                    {
                        if (i != 0) Out.Write(' ');
                        Out.Write(alias.Args[i]);
                    }
                }
            }
            else if (name.EqualsIOC("remove"))
            {
                if (arguments.Count == 1)
                    Configuration.DeleteAlias(arguments[0]);
            }
            else
            {
                if (name.EqualsIOC(Name))
                {
                    Error.WriteLine($"Not allowed to redefine the {Name} built-in command. Here be dragons...");
                    return;
                }
                Configuration.SetAlias(name, arguments);
            }
        }

        public override void PrintUsage()
        {
            Out.WriteLine("ttime alias [remove] [<alias>] [<argument>...]");
            Out.WriteLine();
            Out.WriteLine("    An alias is a shortcut for invoking another commands with specific");
            Out.WriteLine("    arguments. For example 'ttime alias rtj report today format=json' will");
            Out.WriteLine("    create an alias that invokes 'ttime report today format=json'.");
            Out.WriteLine();
            Out.WriteLine("    Invoking the alias command without other arguments will list all aliases");
            Out.WriteLine("    defined.");
            Out.WriteLine();
            Out.WriteLine("    Invoking the alias command with just an alias name will print the");
            Out.WriteLine("    definition of that alias.");
            Out.WriteLine();
            Out.WriteLine("    Invoking with an alias name and at least one additional argument will");
            Out.WriteLine("    create or update an alias.");
            Out.WriteLine();
            Out.WriteLine("    Use 'ttime alias remove <alias>' to delete an alias.");
        }

        public override string Name => "alias";

        public override string Description => "Setup aliases to shorten other commands";
    }
}