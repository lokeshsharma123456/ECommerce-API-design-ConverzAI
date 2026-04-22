using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// MySQL implementation of IProductRepository using EF Core.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        // Detail page → load everything (images, tags, reviews)
        return await _db.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(int page, int size, CancellationToken ct)
    {
        // Grid → minimal data, no Include calls
        return await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> ListByCategoryAsync(
        string category, int page, int size, CancellationToken ct)
    {
        // Grid filtered by category → still minimal data
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.Category == category)
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);
    }
}
