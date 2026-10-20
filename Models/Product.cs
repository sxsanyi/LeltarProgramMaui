using SQLite;

namespace LeltarKezelo.Models;

/// <summary>
/// Egy leltárban szereplő termék. A feladatban megadott négy mező mellett
/// tartalmaz egy elsődleges kulcsot és egy keresésre optimalizált, normalizált névmezőt.
/// </summary>
[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Egyedi vonalkód (adatbázis szinten is egyedi index védi).</summary>
    [NotNull, MaxLength(64), Indexed(Name = "IX_Products_Barcode", Unique = true)]
    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>
    /// A név kisbetűs, ékezet nélküli változata. Erre keresünk, így a keresés
    /// kis-/nagybetű és ékezet független ("kenyer" megtalálja a "Kenyér"-t), és indexelhető.
    /// </summary>
    [NotNull, Indexed(Name = "IX_Products_SearchName")]
    public string SearchName { get; set; } = string.Empty;

    /// <summary>Nincs készleten (a lista ez alapján jelöli pirossal a sort).</summary>
    [Ignore]
    public bool IsOutOfStock => Quantity <= 0;
}
