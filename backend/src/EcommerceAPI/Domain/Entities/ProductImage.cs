namespace EcommerceAPI.Domain.Entities;

/// <summary>
/// One image URL for a product. Child table (1-to-many with Product).
/// </summary>
public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
}
