﻿using System;
using System.Collections.Generic;
using System.Linq;
using ttime.Backends;
using ttime.Backends.LiteDb;
using ttime.Backends.MongoDb;

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

            using var storage = CreateStorage();
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
        catch (TTimeError ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static IStorage CreateStorage()
    {
        Enum.TryParse<TTimeBackend>(Environment.GetEnvironmentVariable("TTIME_BACKEND"), true, out var backend);
        if (backend == TTimeBackend.Mongo)
        {
            var connectionString = Environment.GetEnvironmentVariable("TTIME_MONGO_CONNECTION");
            var database = Environment.GetEnvironmentVariable("TTIME_MONGO_DATABASE") ?? "ttime";
            return new MongoStorage(connectionString, database);
        }

        return new LiteDbStorage();
    }

    public static Command GetCommand(string action)
    {
        return AvailableCommands.FirstOrDefault(c => action.EqualsOIC(c.Name));
    }
}