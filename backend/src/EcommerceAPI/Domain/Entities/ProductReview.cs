namespace EcommerceAPI.Domain.Entities;

/// <summary>
/// One customer review for a product. Child table (1-to-many with Product).
/// </summary>
public class ProductReview
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
}
