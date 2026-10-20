using LeltarKezelo.Models;

namespace LeltarKezelo.Services;

/// <summary>Termékek perzisztenciája. A ViewModelek csak ezen az absztrakción keresztül érik el az adatbázist.</summary>
public interface IProductRepository
{
    /// <summary>Lapozott keresés név (ékezet- és kisbetűfüggetlen) vagy vonalkód részletre, név szerint rendezve.</summary>
    Task<IReadOnlyList<Product>> SearchAsync(string? query, int skip, int take);

    Task<int> CountAsync(string? query);

    Task<Product?> GetByIdAsync(int id);

    Task<bool> BarcodeExistsAsync(string barcode, int? excludeProductId = null);

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(int id);
}
