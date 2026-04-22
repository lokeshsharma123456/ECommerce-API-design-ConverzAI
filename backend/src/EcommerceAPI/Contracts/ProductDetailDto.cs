namespace EcommerceAPI.Contracts;

/// <summary>
/// Full product shape for the detail page.
/// Flattens child tables (Images, Tags) into simple arrays and exposes reviews via ReviewDto.
/// </summary>
public record ProductDetailDto(
    int Id,
    string Title,
    string Description,
    string Category,
    string Brand,
    string Sku,
    decimal Price,
    decimal DiscountPercentage,
    double Rating,
    int Stock,
    double Weight,
    int MinimumOrderQuantity,
    string AvailabilityStatus,
    string WarrantyInformation,
    string ShippingInformation,
    string ReturnPolicy,
    string Thumbnail,
    IReadOnlyList<string> Images,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ReviewDto> Reviews
);
