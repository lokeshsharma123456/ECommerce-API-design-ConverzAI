namespace EcommerceAPI.Domain.Entities.ValueObjects;

/// <summary>
/// Product metadata (barcode, QR, timestamps). Stored as a JSON column on Product.
/// </summary>
public class ProductMeta
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}
