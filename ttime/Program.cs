using System;
using System.IO;
using LiteDB;

namespace ttime
{
    class Program
    {
        private static LiteDatabase _db;

        static void Main(string[] args)
        {
            var dbPath = GetDbPath();
            _db = new LiteDatabase(dbPath);
            using (_db)
            {
                Command command;
                Span<string> remainingArgs;

                if (args.Length == 0)
                {
                    command = new UsageCommand(Console.Out);
                    remainingArgs = new Span<string>();
                }
                else
                {
                    var action = args[0];
                    command = GetCommand(action) ?? new UsageCommand(Console.Out);
                    remainingArgs = args.AsSpan(1);
                }

                command.Run(remainingArgs);
            }
        }

        public static Command GetCommand(string action)
        {
            switch (action?.ToLowerInvariant())
            {
                case "help":
                    return new UsageCommand(Console.Out);
                default:
                    return null;
            }
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