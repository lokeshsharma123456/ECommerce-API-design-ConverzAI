using EcommerceAPI.Application.Services;
using EcommerceAPI.Application.Strategies;
using EcommerceAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    // GET /api/products?page=1&size=20&category=beauty&query=phone
    // Matches spec: /products?query={query} → ES full-text, /products?category={c} → MySQL filter
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? query = null,
        CancellationToken ct = default)
    {
        (page, size) = Clamp(page, size);
        var req = new SearchRequest(Query: query, Category: category, Page: page, Size: size);
        var items = await _service.SearchAsync(req, ct);
        return Ok(items);
    }

    // GET /api/products/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // GET /api/products/search?q=phone&category=smartphones&page=1&size=20
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> Search(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        (page, size) = Clamp(page, size);
        var req = new SearchRequest(query, category, page, size);
        var items = await _service.SearchAsync(req, ct);
        return Ok(items);
    }

    // GET /api/products/categories
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<string>>> Categories(CancellationToken ct)
    {
        var categories = await _service.GetCategoriesAsync(ct);
        return Ok(categories);
    }

    private static (int page, int size) Clamp(int page, int size)
    {
        const int MaxSize = 100;
        if (page < 1) page = 1;
        if (size < 1 || size > MaxSize) size = 20;
        return (page, size);
    }
}


