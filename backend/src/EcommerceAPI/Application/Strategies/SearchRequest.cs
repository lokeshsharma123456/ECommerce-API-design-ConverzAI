namespace EcommerceAPI.Application.Strategies;

/// <summary>
/// Input to all search strategies. Carries every possible search/filter parameter.
/// Each strategy inspects this and decides if it can handle the request.
/// </summary>
public record SearchRequest(
    string? Query,
    string? Category,
    int Page = 1,
    int Size = 20
);
