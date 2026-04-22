namespace EcommerceAPI.Contracts;

/// <summary>
/// Generic wrapper for paginated responses. Frontend uses Total to render "Page X of Y".
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int Total
);
