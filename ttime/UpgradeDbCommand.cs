using System;
using System.IO;
using LiteDB;
using ttime.Backends.LiteDb;

namespace ttime;

public class UpgradeDbCommand : Command
{
    public override void Run(Span<string> args)
    {
        var dbFolder = Path.GetDirectoryName(DbPath) ?? throw new InvalidOperationException("Unable to determine data folder path");
        var backupDataFile = Path.Combine(dbFolder, $"data-{DateTime.Now:yyyyMMdd-HHmmss}.litedb");
        File.Copy(DbPath, backupDataFile);

        using var originalDb = new LiteDatabase(new ConnectionString
        {
            Filename = DbPath,
            Upgrade = true,
        });
        var upgradeDbPath = $"{DbPath}.new";
        if (File.Exists(upgradeDbPath))
            File.Delete(upgradeDbPath);
        using var newdb = new LiteDatabase(new ConnectionString
        {
            Filename = upgradeDbPath,
        });

        var storage = new LiteDbStorage(newdb);
        storage.Import(originalDb);
        originalDb.Dispose();
        newdb.Dispose();
        File.Delete(DbPath);
        File.Move(upgradeDbPath, DbPath);
    }

    public override void PrintUsage()
    {
        Out.WriteLine("ttime upgrade-db");
        Out.WriteLine();
        Out.WriteLine("    Upgrade from a previous version of lite-db. The original data file will be");
        Out.WriteLine(@"    archived in the ttime data directory (~/.ttime or ~\AppData\Roaming\ttime)");
    }

    public override string Name => "upgrade-db";
    public override string Description => "Upgrade your database from a previous version of lite-db";
    public string DbPath { get; set; }
}