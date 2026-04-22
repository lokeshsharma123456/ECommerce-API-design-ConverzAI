using EcommerceAPI.Domain.Entities.ValueObjects;

namespace EcommerceAPI.Domain.Entities;

/// <summary>
/// Core product entity. Maps to the `products` table in MySQL.
/// Scalar fields → columns. Nested objects → JSON columns. Arrays → child tables.
/// </summary>
public class Product
{
    // Primary key
    public int Id { get; set; }

    // Scalar text fields
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    // Scalar numeric fields
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
    public double Weight { get; set; }
    public int MinimumOrderQuantity { get; set; }

    // Scalar string fields (misc)
    public string AvailabilityStatus { get; set; } = string.Empty;
    public string WarrantyInformation { get; set; } = string.Empty;
    public string ShippingInformation { get; set; } = string.Empty;
    public string ReturnPolicy { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;

    // Nested objects → stored as JSON columns
    public Dimensions? Dimensions { get; set; }
    public ProductMeta? Meta { get; set; }

    // Arrays → child tables (navigation properties)
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductTag> Tags { get; set; } = new();
    public List<ProductReview> Reviews { get; set; } = new();
}
