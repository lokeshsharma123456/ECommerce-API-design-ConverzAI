using EcommerceAPI.Contracts;
using EcommerceAPI.Infrastructure.Persistence.Repositories;

namespace EcommerceAPI.Application.Strategies;

/// <summary>
/// Handles requests WITHOUT a full-text query — plain listing or category filtering.
/// Translates SearchRequest into specific IProductRepository calls, then maps to DTOs.
/// </summary>
public class MySqlSearchStrategy : ISearchStrategy
{
    private readonly IProductRepository _repo;

    public MySqlSearchStrategy(IProductRepository repo)
    {
        _repo = repo;
    }

    public bool CanHandle(SearchRequest request)
        => string.IsNullOrWhiteSpace(request.Query);

    public async Task<IReadOnlyList<ProductListItemDto>> SearchAsync(SearchRequest request, CancellationToken ct)
    {
        var items = !string.IsNullOrWhiteSpace(request.Category)
            ? await _repo.ListByCategoryAsync(request.Category, request.Page, request.Size, ct)
            : await _repo.ListAsync(request.Page, request.Size, ct);

        return items.Select(p => p.ToListItemDto()).ToList();
    }
}
