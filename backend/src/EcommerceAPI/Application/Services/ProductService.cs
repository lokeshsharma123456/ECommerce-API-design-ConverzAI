using EcommerceAPI.Application.Strategies;
using EcommerceAPI.Contracts;
using EcommerceAPI.Infrastructure.Persistence.Repositories;

namespace EcommerceAPI.Application.Services;

/// <summary>
/// Default implementation of IProductService.
/// - Direct lookups (categories, get-by-id) → repository, then mapped to DTO.
/// - Search/list → first matching ISearchStrategy (strategies already return DTOs).
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IEnumerable<ISearchStrategy> _strategies;

    public ProductService(IProductRepository repo, IEnumerable<ISearchStrategy> strategies)
    {
        _repo = repo;
        _strategies = strategies;
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct)
        => _repo.GetCategoriesAsync(ct);

    public async Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        return product?.ToDetailDto();
    }

    public Task<IReadOnlyList<ProductListItemDto>> SearchAsync(SearchRequest request, CancellationToken ct)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(request))
            ?? throw new InvalidOperationException(
                $"No ISearchStrategy can handle request: Query='{request.Query}', Category='{request.Category}'.");

        return strategy.SearchAsync(request, ct);
    }
}
