namespace EcommerceAPI.Infrastructure.Seeding;

/// <summary>
/// Raw DTOs that mirror the DummyJSON /products response shape.
/// NOT domain entities — just a deserialization buffer.
/// JsonPropertyName isn't needed because we'll configure System.Text.Json with
/// PropertyNameCaseInsensitive = true, and DummyJSON fields are camelCase that
/// line up with our PascalCase props after case-insensitive matching.
/// </summary>
public class DummyJsonResponse
{
    public List<DummyJsonProduct> Products { get; set; } = new();
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
}

public class DummyJsonProduct
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Brand { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public double Weight { get; set; }
    public DummyJsonDimensions? Dimensions { get; set; }
    public string WarrantyInformation { get; set; } = string.Empty;
    public string ShippingInformation { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
    public List<DummyJsonReview> Reviews { get; set; } = new();
    public string ReturnPolicy { get; set; } = string.Empty;
    public int MinimumOrderQuantity { get; set; }
    public DummyJsonMeta? Meta { get; set; }
    public List<string> Images { get; set; } = new();
    public string Thumbnail { get; set; } = string.Empty;
}

public class DummyJsonDimensions
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double Depth { get; set; }
}

public class DummyJsonReview
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
}

public class DummyJsonMeta
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}
