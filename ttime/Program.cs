using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace ttime;

class Program
{
    private static LiteDatabase _db;

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

    static void Main(string[] args)
    {
        var requestedCommandName = args.Length > 0 ? args[0] : "help";
        var command = GetCommand(requestedCommandName);
        if (command is UpgradeDbCommand upgradeCommand)
        {
            upgradeCommand.DbPath = GetDbPath();
            command.Run(new Span<string>());
            return;
        }

        var dbPath = GetDbPath();
        try
        {
            _db = new LiteDatabase(new ConnectionString
            {
                Filename = dbPath,
                Upgrade = false,
            });
        }
        catch (LiteException ex) when (ex.ErrorCode == 103)
        {
            Console.Error.WriteLine($"Data file is out-of-date. Please run '{new UpgradeDbCommand().Name}' to upgrade data file to the current version.");
            return;
        }

        using (_db)
        {
            var storage = new Storage(_db);
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
        }
    }

    public static Command GetCommand(string action)
    {
        return AvailableCommands.FirstOrDefault(c => action.EqualsOIC(c.Name));
    }

    private static string GetDbPath()
    {
        var dbPath = Environment.GetEnvironmentVariable("TTIME_DATA");
        string dbFolder;
        if (string.IsNullOrEmpty(dbPath))
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                dbFolder = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".ttime");
            }
            else
            {
                dbFolder = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create);
                dbFolder = Path.Combine(dbFolder, "ttime");
            }

            dbPath = Path.Combine(dbFolder, "data.litedb");
        }
        else
        {
            dbFolder = Path.GetDirectoryName(dbPath);
        }

        if (!Directory.Exists(dbFolder))
            Directory.CreateDirectory(dbFolder);
        return dbPath;
    }
}