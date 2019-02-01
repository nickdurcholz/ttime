using System;
using System.IO;
using LiteDB;

namespace ttime
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbPath = GetDbPath();
            using (var db = new LiteDatabase(dbPath))
            {
                var configuration = new Configuration(db);
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