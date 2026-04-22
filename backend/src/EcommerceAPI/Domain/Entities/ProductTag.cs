namespace EcommerceAPI.Domain.Entities;

/// <summary>
/// One tag for a product (e.g. "beauty", "mascara"). Child table (1-to-many with Product).
/// </summary>
public class ProductTag
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Tag { get; set; } = string.Empty;
}
