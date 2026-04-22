namespace EcommerceAPI.Domain.Entities.ValueObjects;

/// <summary>
/// Physical dimensions of a product. Stored as a JSON column on Product.
/// Value Object = no Id, identified by its values.
/// </summary>
public class Dimensions
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double Depth { get; set; }
}
