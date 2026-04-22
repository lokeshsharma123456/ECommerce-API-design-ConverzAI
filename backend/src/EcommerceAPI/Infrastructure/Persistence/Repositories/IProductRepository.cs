using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// Contract for reading products from the data store.
/// Services depend on this interface, not on EF Core / MySQL directly.
/// </summary>
public interface IProductRepository
{
    /// <summary>Distinct list of all category names.</summary>
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct);

    /// <summary>Single product by id, or null if not found.</summary>
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Paginated list of all products.</summary>
    Task<IReadOnlyList<Product>> ListAsync(int page, int size, CancellationToken ct);

    /// <summary>Paginated list of products in a specific category.</summary>
    Task<IReadOnlyList<Product>> ListByCategoryAsync(string category, int page, int size, CancellationToken ct);
}
