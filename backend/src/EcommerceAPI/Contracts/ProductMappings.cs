using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Contracts;

/// <summary>
/// Extension methods mapping Domain entities to public DTOs.
/// Keeps services/strategies focused; all mapping lives here.
/// </summary>
public static class ProductMappings
{
    public static ProductListItemDto ToListItemDto(this Product p) =>
        new(p.Id, p.Title, p.Brand, p.Category, p.Price, p.DiscountPercentage, p.Rating, p.Thumbnail);

    public static ProductDetailDto ToDetailDto(this Product p) =>
        new(
            p.Id, p.Title, p.Description, p.Category, p.Brand, p.Sku,
            p.Price, p.DiscountPercentage, p.Rating, p.Stock, p.Weight, p.MinimumOrderQuantity,
            p.AvailabilityStatus, p.WarrantyInformation, p.ShippingInformation, p.ReturnPolicy,
            p.Thumbnail,
            p.Images.Select(i => i.Url).ToList(),
            p.Tags.Select(t => t.Tag).ToList(),
            p.Reviews.Select(r => r.ToDto()).ToList()
        );

    public static ReviewDto ToDto(this ProductReview r) =>
        new(r.Rating, r.Comment, r.Date, r.ReviewerName, r.ReviewerEmail);
}
