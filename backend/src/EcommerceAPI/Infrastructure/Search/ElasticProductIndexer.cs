using EcommerceAPI.Contracts;
using EcommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace EcommerceAPI.Infrastructure.Search;

/// <summary>
/// Builds and maintains the Elasticsearch `products` index.
/// Called on startup after MySQL is seeded.
/// </summary>
public class ElasticProductIndexer
{
    private readonly IElasticClient _client;
    private readonly AppDbContext _db;
    private readonly string _indexName;
    private readonly ILogger<ElasticProductIndexer> _log;

    public ElasticProductIndexer(
        IElasticClient client,
        AppDbContext db,
        IConfiguration config,
        ILogger<ElasticProductIndexer> log)
    {
        _client = client;
        _db = db;
        _indexName = config["Elasticsearch:Index"] ?? "products";
        _log = log;
    }

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var exists = await _client.Indices.ExistsAsync(_indexName, ct: ct);
        if (exists.Exists)
        {
            _log.LogInformation("ES: index {Index} already exists; skipping create.", _indexName);
            return;
        }

        var create = await _client.Indices.CreateAsync(_indexName, c => c
            .Map<ProductSearchDoc>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t.Name(n => n.Title).Fields(f => f.Keyword(kw => kw.Name("keyword"))))
                    .Text(t => t.Name(n => n.Description))
                    .Keyword(k => k.Name(n => n.Category))
                    .Keyword(k => k.Name(n => n.Brand))
                    .Keyword(k => k.Name(n => n.Tags))
                    .Number(nm => nm.Name(n => n.Price).Type(NumberType.Double))
                    .Number(nm => nm.Name(n => n.Rating).Type(NumberType.Double))
                    .Number(nm => nm.Name(n => n.DiscountPercentage).Type(NumberType.Double))
                    .Keyword(k => k.Name(n => n.Thumbnail).Index(false))
                )
            ), ct);

        if (!create.IsValid)
            throw new InvalidOperationException($"Failed to create ES index: {create.DebugInformation}");

        _log.LogInformation("ES: created index {Index}.", _indexName);
    }

    public async Task BulkIndexAllAsync(CancellationToken ct = default)
    {
        // Skip if already populated
        var count = await _client.CountAsync<ProductSearchDoc>(c => c.Index(_indexName), ct);
        if (count.IsValid && count.Count > 0)
        {
            _log.LogInformation("ES: index {Index} already has {Count} docs; skipping bulk index.", _indexName, count.Count);
            return;
        }

        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Tags)
            .ToListAsync(ct);

        if (products.Count == 0)
        {
            _log.LogInformation("ES: no products in MySQL to index.");
            return;
        }

        var docs = products.Select(p => new ProductSearchDoc
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Category = p.Category,
            Brand = p.Brand,
            Price = p.Price,
            Rating = p.Rating,
            Thumbnail = p.Thumbnail,
            DiscountPercentage = p.DiscountPercentage,
            Tags = p.Tags.Select(t => t.Tag).ToList()
        }).ToList();

        var bulk = await _client.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(docs, (desc, doc) => desc.Id(doc.Id)), ct);

        if (bulk.Errors)
            throw new InvalidOperationException($"ES bulk index had errors: {bulk.DebugInformation}");

        _log.LogInformation("ES: indexed {Count} products into {Index}.", docs.Count, _indexName);
    }

    public async Task<IReadOnlyList<ProductListItemDto>> SearchAsync(
        string query, int page, int size, CancellationToken ct = default)
    {
        var from = (page - 1) * size;

        var response = await _client.SearchAsync<ProductSearchDoc>(s => s
            .Index(_indexName)
            .From(from)
            .Size(size)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(query)
                    .Fields(f => f
                        .Field(p => p.Title, boost: 3)
                        .Field(p => p.Tags, boost: 2)
                        .Field(p => p.Brand, boost: 2)
                        .Field(p => p.Category, boost: 1.5)
                        .Field(p => p.Description))
                    .Fuzziness(Fuzziness.Auto)
                    .Type(TextQueryType.BestFields)
                )
            ), ct);

        if (!response.IsValid)
            throw new InvalidOperationException($"ES search failed: {response.DebugInformation}");

        return response.Documents
            .Select(d => new ProductListItemDto(
                d.Id, d.Title, d.Brand, d.Category, d.Price, d.DiscountPercentage, d.Rating, d.Thumbnail))
            .ToList();
    }
}
