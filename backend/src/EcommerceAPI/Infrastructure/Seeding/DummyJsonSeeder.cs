using System.Net.Http.Json;
using System.Text.Json;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Entities.ValueObjects;
using EcommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Seeding;

/// <summary>
/// On first startup, fetches products from dummyjson.com and saves them to MySQL.
/// Idempotent: skips if the Products table already has data.
/// </summary>
public class DummyJsonSeeder
{
    private const string DummyJsonUrl = "https://dummyjson.com/products?limit=0";

    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<DummyJsonSeeder> _log;

    public DummyJsonSeeder(
        AppDbContext db,
        IHttpClientFactory httpFactory,
        ILogger<DummyJsonSeeder> log)
    {
        _db = db;
        _httpFactory = httpFactory;
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // 1. Guard — skip if already seeded
        if (await _db.Products.AnyAsync(ct))
        {
            _log.LogInformation("Seeder: products table not empty; skipping.");
            return;
        }

        _log.LogInformation("Seeder: fetching products from {Url}", DummyJsonUrl);

        // 2. Fetch + deserialize
        var http = _httpFactory.CreateClient();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var payload = await http.GetFromJsonAsync<DummyJsonResponse>(DummyJsonUrl, options, ct);

        if (payload is null || payload.Products.Count == 0)
        {
            _log.LogWarning("Seeder: empty response from DummyJSON.");
            return;
        }

        // 3. Map raw DTOs → domain entities
        var entities = payload.Products.Select(Map).ToList();

        // 4. Persist in a single transaction
        _db.Products.AddRange(entities);
        await _db.SaveChangesAsync(ct);

        _log.LogInformation("Seeder: inserted {Count} products.", entities.Count);
    }

    private static Product Map(DummyJsonProduct src) => new()
    {
        Id = src.Id,
        Title = src.Title,
        Description = src.Description,
        Category = src.Category,
        Brand = src.Brand,
        Sku = src.Sku,
        Price = src.Price,
        DiscountPercentage = src.DiscountPercentage,
        Rating = src.Rating,
        Stock = src.Stock,
        Weight = src.Weight,
        MinimumOrderQuantity = src.MinimumOrderQuantity,
        AvailabilityStatus = src.AvailabilityStatus,
        WarrantyInformation = src.WarrantyInformation,
        ShippingInformation = src.ShippingInformation,
        ReturnPolicy = src.ReturnPolicy,
        Thumbnail = src.Thumbnail,
        Dimensions = src.Dimensions is null ? null : new Dimensions
        {
            Width = src.Dimensions.Width,
            Height = src.Dimensions.Height,
            Depth = src.Dimensions.Depth
        },
        Meta = src.Meta is null ? null : new ProductMeta
        {
            CreatedAt = src.Meta.CreatedAt,
            UpdatedAt = src.Meta.UpdatedAt,
            Barcode = src.Meta.Barcode,
            QrCode = src.Meta.QrCode
        },
        Images = src.Images.Select(url => new ProductImage { Url = url }).ToList(),
        Tags = src.Tags.Select(t => new ProductTag { Tag = t }).ToList(),
        Reviews = src.Reviews.Select(r => new ProductReview
        {
            Rating = r.Rating,
            Comment = r.Comment,
            Date = r.Date,
            ReviewerName = r.ReviewerName,
            ReviewerEmail = r.ReviewerEmail
        }).ToList()
    };
}
