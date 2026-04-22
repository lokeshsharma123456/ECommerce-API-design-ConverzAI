namespace EcommerceAPI.Contracts;

/// <summary>
/// One customer review — nested inside ProductDetailDto.
/// </summary>
public record ReviewDto(
    int Rating,
    string Comment,
    DateTime Date,
    string ReviewerName,
    string ReviewerEmail
);
