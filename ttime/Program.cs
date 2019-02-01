using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FileMode = LiteDB.FileMode;

namespace ttime
{
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
        };

        static void Main(string[] args)
        {
            var dbPath = GetDbPath();
            _db = new LiteDatabase(new ConnectionString
            {
                Filename = dbPath,
                Mode = FileMode.Exclusive,
                Upgrade = true,
            });
            using (_db)
            {
                foreach (var c in AvailableCommands)
                {
                    c.Db = _db;
                    c.Out = Console.Out;
                }

                var requestedCommandName = args.Length > 0 ? args[0] : "help";
                var command = GetCommand(requestedCommandName);
                if (command == null)
                {
                    Console.Error.WriteLine("Command not found: " + requestedCommandName);
                    command = GetCommand("help");
                }
                var remainingArgs = args.Length > 0 ? args.AsSpan(1) : new Span<string>();

                command.Run(remainingArgs);
            }
        }

        public static Command GetCommand(string action)
        {
            return AvailableCommands.FirstOrDefault(c => action.EqualsIOC(c.Name));
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
                dbFolder = Path.GetFileName(dbPath);
            }

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            return dbPath;
        }
    }
}