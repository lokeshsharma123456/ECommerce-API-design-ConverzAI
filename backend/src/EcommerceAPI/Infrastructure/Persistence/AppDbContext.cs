using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence;

/// <summary>
/// EF Core bridge between C# entities and the MySQL database.
/// Each DbSet = one table.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(e =>
        {
            // decimal precision for money fields (MySQL DECIMAL(18,2))
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.DiscountPercentage).HasPrecision(5, 2);

            // Value Objects → inlined as columns on Products table
            // (Pomelo MySQL doesn't support EF Core's .ToJson() yet; flat columns work fine.)
            e.OwnsOne(p => p.Dimensions);
            e.OwnsOne(p => p.Meta);

            // 1-to-many relationships (cascade delete children when product deleted)
            e.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(p => p.Tags)
                .WithOne()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(p => p.Reviews)
                .WithOne()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
