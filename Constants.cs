using SQLite;
using System.IO;

namespace ADHDCompanionApp;

public static class Constants
{
    public const string DatabaseFilename = "adhdcompanion.db3";

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}