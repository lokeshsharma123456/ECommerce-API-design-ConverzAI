namespace EcommerceAPI.Contracts;

/// <summary>
/// Slim product shape for grid/listing. Only the fields the UI shows per card.
/// </summary>
public record ProductListItemDto(
    int Id,
    string Title,
    string Brand,
    string Category,
    decimal Price,
    decimal DiscountPercentage,
    double Rating,
    string Thumbnail
);
