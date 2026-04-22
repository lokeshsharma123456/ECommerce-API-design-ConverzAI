using EcommerceAPI.Application.Strategies;
using EcommerceAPI.Contracts;

namespace EcommerceAPI.Application.Services;

/// <summary>
/// Orchestrates product-related use cases. Always returns DTOs, never entities.
/// Direct lookups go to the repository. Search/list goes through the strategy pattern.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct);
    Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ProductListItemDto>> SearchAsync(SearchRequest request, CancellationToken ct);
}
