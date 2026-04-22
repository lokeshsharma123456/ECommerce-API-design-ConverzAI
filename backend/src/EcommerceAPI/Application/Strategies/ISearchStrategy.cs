using EcommerceAPI.Contracts;

namespace EcommerceAPI.Application.Strategies;

/// <summary>
/// Contract for a search backend (MySQL, Elasticsearch, Redis, ...).
/// Service injects all strategies and picks the first that CanHandle the request.
/// Adding a new backend = new class only, no changes to Service (Open/Closed).
/// </summary>
public interface ISearchStrategy
{
    /// <summary>Does this strategy know how to handle the given request?</summary>
    bool CanHandle(SearchRequest request);

    /// <summary>Execute the search and return matching products as list-item DTOs.</summary>
    Task<IReadOnlyList<ProductListItemDto>> SearchAsync(SearchRequest request, CancellationToken ct);
}
