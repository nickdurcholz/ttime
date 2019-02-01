using System;
using System.IO;
using LiteDB;

namespace ttime
{
    class Program
    {
        static void Main(string[] args)
        {
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            string dbPath = Path.Combine(homePath, ".ttime");
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);
            dbPath = Path.Combine(dbPath, "data.litedb");
            using (var db = new LiteDatabase(dbPath))
            {
                if (!db.CollectionExists("entries"))
                {
                    CreateCollection(db);
                }
            }
        }
    }
}
