using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Models;

namespace ProductMicroservice.API.Data;

/// <summary>
/// Contexte de base de données pour l'application
/// </summary>
public class ProductContext : DbContext
{
    /// <summary>
    /// Constructeur du contexte de base de données
    /// </summary>
    /// <param name="options">Options de configuration pour Entity Framework Core</param>
    public ProductContext(DbContextOptions<ProductContext> options) : base(options)
    {
        // Only create if it doesn't exist
        Database.EnsureCreated();
    }

    /// <summary>
    /// Collection des produits dans la base de données
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Collection des catégories dans la base de données
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Configure les relations et initialise les données de base
    /// </summary>
    /// <param name="modelBuilder">Le constructeur de modèle utilisé pour la configuration</param>
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