using System;
using System.Collections.Generic;
using System.Linq;
using CommandDotNet;

namespace ttime;

class Program
{
    public static readonly IReadOnlyList<Command> AvailableCommands = new List<Command>
    {
        new HelpCommand(),
        new StartCommand(),
        new StopCommand(),
        new ReportCommand(),
        new ConfigCommand(),
        new ImportCommand(),
        new ExportCommand(),
        new AliasCommand(),
        new RemoveCommand(),
        new StopTimeCommand(),
        new UpgradeDbCommand(),
        new EditCommand(),
    };

    [Subcommand] public HelpCommand HelpCommand { get; set; }
    [Subcommand] public StartCommand StartCommand { get; set; }
    [Subcommand] public StopCommand StopCommand { get; set; }
    [Subcommand] public ReportCommand ReportCommand { get; set; }
    [Subcommand] public ConfigCommand ConfigCommand { get; set; }
    [Subcommand] public ImportCommand ImportCommand { get; set; }
    [Subcommand] public ExportCommand ExportCommand { get; set; }
    [Subcommand] public AliasCommand AliasCommand { get; set; }
    [Subcommand] public RemoveCommand RemoveCommand { get; set; }
    [Subcommand] public StopTimeCommand StopTimeCommand { get; set; }
    [Subcommand] public UpgradeDbCommand UpgradeDbCommand { get; set; }
    [Subcommand] public EditCommand EditCommand { get; set; }

    static int Main(string[] args)
    {
        try
        {
            var requestedCommandName = args.Length > 0 ? args[0] : "help";
            var command = GetCommand(requestedCommandName);
            if (command is UpgradeDbCommand upgradeCommand)
            {
                upgradeCommand.DbPath = LiteDbStorage.GetDbPath();
                command.Run(new Span<string>());
                return 0;
            }

            using var storage = new LiteDbStorage();
            var configuration = new Configuration(storage);
            foreach (var c in AvailableCommands)
            {
                c.Configuration = configuration;
                c.Storage = storage;
                c.Out = Console.Out;
                c.Error = Console.Error;
                c.In = Console.In;
            }

            var alias = configuration.Aliases.FirstOrDefault(a => a.Name.EqualsOIC(requestedCommandName));
            if (alias != null)
                requestedCommandName = alias.Args[0];

            command ??= GetCommand(requestedCommandName);
            if (command == null)
            {
                Console.Error.WriteLine("Command not found: " + requestedCommandName);
                command = GetCommand("help");
            }

            var remainingArgs = args.Length > 0 ? args.AsSpan(1) : new Span<string>();

            if (alias != null && alias.Args.Count > 1)
            {
                var a = new string[alias.Args.Count + remainingArgs.Length - 1];
                int i = 0;
                for (var j = 1; j < alias.Args.Count; j++)
                    a[i++] = alias.Args[j];
                for (var j = 0; j < remainingArgs.Length; j++)
                    a[i++] = remainingArgs[j];

                remainingArgs = a;
            }

            command.Run(remainingArgs);
            return 0;
        }
        catch (CommandError ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    public static Command GetCommand(string action)
    {
        return AvailableCommands.FirstOrDefault(c => action.EqualsOIC(c.Name));
    }
}