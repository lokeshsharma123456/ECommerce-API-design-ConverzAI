namespace EcommerceAPI.Infrastructure.Search;

/// <summary>
/// Document shape stored in the Elasticsearch `products` index.
/// Flat + searchable — no nested child tables. Big/heavy fields (reviews, etc.) stay in MySQL.
/// </summary>
public class ProductSearchDoc
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public string? Thumbnail { get; set; }
    public List<string> Tags { get; set; } = new();
    public decimal DiscountPercentage { get; set; }
}
