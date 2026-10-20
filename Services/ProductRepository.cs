using LeltarKezelo.Data;
using LeltarKezelo.Helpers;
using LeltarKezelo.Models;
using SQLite;

namespace LeltarKezelo.Services;

/// <summary>
/// SQLite alapú repository (sqlite-net-pcl). A kapcsolat lustán, szálbiztosan inicializálódik
/// az első használatkor: tábla + indexek létrehozása, igény esetén demóadatok betöltése.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private const string TableName = "Products";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public async Task<IReadOnlyList<Product>> SearchAsync(string? query, int skip, int take)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        var (where, args) = BuildFilter(query);

        // A SearchName indexelt, így a rendezés lapozásnál is olcsó.
        var sql = $"SELECT * FROM {TableName}{where} ORDER BY SearchName, Id LIMIT ? OFFSET ?";
        var parameters = args.Concat(new object[] { take, skip }).ToArray();

        return await db.QueryAsync<Product>(sql, parameters).ConfigureAwait(false);
    }

    public async Task<int> CountAsync(string? query)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        var (where, args) = BuildFilter(query);
        return await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {TableName}{where}", args).ConfigureAwait(false);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        return await db.FindAsync<Product>(id).ConfigureAwait(false);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeProductId = null)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        var count = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {TableName} WHERE Barcode = ? AND Id <> ?",
            barcode.Trim(),
            excludeProductId ?? 0).ConfigureAwait(false);
        return count > 0;
    }

    public async Task AddAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        var db = await GetConnectionAsync().ConfigureAwait(false);
        product.SearchName = TextNormalizer.ForSearch(product.Name);
        await db.InsertAsync(product).ConfigureAwait(false); // AutoIncrement → product.Id kitöltődik
    }

    public async Task UpdateAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        var db = await GetConnectionAsync().ConfigureAwait(false);
        product.SearchName = TextNormalizer.ForSearch(product.Name);

        var affected = await db.UpdateAsync(product).ConfigureAwait(false);
        if (affected == 0)
        {
            throw new InvalidOperationException("A termék már nem létezik az adatbázisban.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        await db.DeleteAsync<Product>(id).ConfigureAwait(false);
    }

    private static (string Where, object[] Args) BuildFilter(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (string.Empty, Array.Empty<object>());
        }

        var namePattern = "%" + TextNormalizer.EscapeLike(TextNormalizer.ForSearch(query)) + "%";
        var barcodePattern = "%" + TextNormalizer.EscapeLike(query.Trim()) + "%";

        return (" WHERE SearchName LIKE ? ESCAPE '\\' OR Barcode LIKE ? ESCAPE '\\'",
            new object[] { namePattern, barcodePattern });
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_connection is not null)
            {
                return _connection;
            }

            // Az inicializálás (tábla létrehozása, demóadatok generálása és beszúrása) teljes egészében
            // háttérszálon fut, hogy a UI szál egy pillanatra se blokkolódjon (Androidon ez ANR-t okozna).
            _connection = await Task.Run(InitializeDatabaseAsync).ConfigureAwait(false);
            return _connection;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task<SQLiteAsyncConnection> InitializeDatabaseAsync()
    {
        var db = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
        await db.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
        await db.CreateTableAsync<Product>().ConfigureAwait(false);

        if (DatabaseConstants.SeedDemoData && await db.Table<Product>().CountAsync().ConfigureAwait(false) == 0)
        {
            var demoProducts = DemoDataGenerator.Create(DatabaseConstants.DemoProductCount);
            // InsertAllAsync - 1 tranzakcióban kerül feltöltésre az adattábla 
            await db.InsertAllAsync(demoProducts).ConfigureAwait(false);
        }

        return db;
    }
}
