using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Models;

namespace ProductMicroservice.Data;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options) : base(options)
    {

    }

    public virtual DbSet<Product> Products => Set<Product>();

    public virtual DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Initialisation des catégories de base
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic Items" },
            new Category { Id = 2, Name = "Clothes", Description = "Dresses" },
            new Category { Id = 3, Name = "Grocery", Description = "Grocery Items" }
        );
    }
}