using EcommerceAPI.Contracts;
using EcommerceAPI.Infrastructure.Search;

namespace EcommerceAPI.Application.Strategies;

/// <summary>
/// Full-text search via Elasticsearch. Triggers when the caller provides a `q` query.
/// </summary>
public class ElasticSearchStrategy : ISearchStrategy
{
    private readonly ElasticProductIndexer _indexer;

    public ElasticSearchStrategy(ElasticProductIndexer indexer)
    {
        _indexer = indexer;
    }

    public bool CanHandle(SearchRequest request)
        => !string.IsNullOrWhiteSpace(request.Query);

    public Task<IReadOnlyList<ProductListItemDto>> SearchAsync(SearchRequest request, CancellationToken ct)
        => _indexer.SearchAsync(request.Query!, request.Page, request.Size, ct);
}
