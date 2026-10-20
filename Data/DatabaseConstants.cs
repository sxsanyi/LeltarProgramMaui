using Microsoft.Maui.Storage;
using SQLite;

namespace LeltarKezelo.Data;

public static class DatabaseConstants
{
    public const string DatabaseFilename = "leltar.db3";

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.FullMutex;

    public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    /// <summary>
    /// Első indításkor (üres adatbázisnál) demóadatokkal tölti fel az adatbázist,
    /// hogy a lista teljesítménye több ezer elemmel is kipróbálható legyen.
    /// </summary>
    public static readonly bool SeedDemoData = true;

    public static readonly int DemoProductCount = 5000;
}
