using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductMicroservice.API.Data;
using ProductMicroservice.Models;
using ProductMicroservice.Repository;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProductMicroservice.Tests;

/// <summary>
/// Tests unitaires pour le repository de produits
/// Vérifie les opérations de base de données avec une base de données en mémoire
/// </summary>
public class ProductRepositoryTests : IDisposable
{
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;
    private readonly ProductContext _context;
    private readonly ProductRepository _repository;

    /// <summary>
    /// Constructeur initialisant la base de données en mémoire avec des données de test
    /// </summary>
    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: $"TestProductDb_{Guid.NewGuid()}")
            .Options;

        _mockLogger = new Mock<ILogger<ProductRepository>>();
        _context = new ProductContext(options);
        _repository = new ProductRepository(_context, _mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated(); // This will trigger OnModelCreating and seed default categories

        // Get the Electronics and Clothes categories that were seeded by OnModelCreating
        var electronics = _context.Categories.First(c => c.Name == "Electronics");
        var clothes = _context.Categories.First(c => c.Name == "Clothes");

        _context.Products.AddRange(
            new Product { Name = "Test Product 1", Description = "Description 1", Price = 99.99m, CategoryId = electronics.Id },
            new Product { Name = "Test Product 2", Description = "Description 2", Price = 149.99m, CategoryId = clothes.Id }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void GetProducts_ShouldReturnAllProducts()
    {
        // Act
        var result = _repository.GetProducts();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, product => Assert.NotNull(product.Name));
    }

    [Fact]
    public void GetProductById_ShouldReturnProduct()
    {
        // Arrange
        var existingProduct = _context.Products.First();

        // Act
        var result = _repository.GetProductById(existingProduct.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProduct.Id, result.Id);
        Assert.Equal(existingProduct.Name, result.Name);
    }

    [Fact]
    public void AddProduct_ShouldAddNewProduct()
    {
        // Arrange
        var category = _context.Categories.First();
        var product = new Product { 
            Name = "New Test Product", 
            Description = "New Description", 
            Price = 10.0m, 
            CategoryId = category.Id 
        };

        // Act
        var result = _repository.AddProduct(product);
        _repository.SaveChanges();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.CategoryId, result.CategoryId);

        // Verify it's in the database
        var savedProduct = _context.Products.Find(result.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(product.Name, savedProduct.Name);
    }

    [Fact]
    public void GetCategories_ShouldReturnAllCategories()
    {
        // Act
        var result = _repository.GetCategories();

        // Assert
        Assert.Equal(3, result.Count()); // Expect 3 default categories
        Assert.Contains(result, c => c.Name == "Electronics");
        Assert.Contains(result, c => c.Name == "Clothes");
        Assert.Contains(result, c => c.Name == "Grocery");
    }

    [Fact]
    public void GetCategoryById_ShouldReturnCategory()
    {
        // Arrange
        var existingCategory = _context.Categories.First();

        // Act
        var result = _repository.GetCategoryById(existingCategory.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingCategory.Id, result.Id);
        Assert.Equal(existingCategory.Name, result.Name);
    }

    [Fact]
    public void AddCategory_ShouldAddNewCategory()
    {
        // Arrange
        var category = new Category { 
            Name = "New Test Category", 
            Description = "New Test Description" 
        };

        // Act
        var result = _repository.AddCategory(category);
        _repository.SaveChanges();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(category.Name, result.Name);

        // Verify it's in the database
        var savedCategory = _context.Categories.Find(result.Id);
        Assert.NotNull(savedCategory);
        Assert.Equal(category.Name, savedCategory.Name);
    }

    [Fact]
    public void GetProductById_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = _repository.GetProductById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdateProduct_ModifiesExistingProduct()
    {
        // Arrange
        var existingProduct = _context.Products.First();
        existingProduct.Price = 299.99m;
        existingProduct.Description = "Updated Description";

        // Act
        _repository.UpdateProduct(existingProduct);
        _repository.SaveChanges();

        // Assert
        var updatedProduct = _context.Products.Find(existingProduct.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(299.99m, updatedProduct.Price);
        Assert.Equal("Updated Description", updatedProduct.Description);
    }

    [Fact]
    public void DeleteProduct_RemovesProduct()
    {
        // Arrange
        var existingProduct = _context.Products.First();

        // Act
        _repository.DeleteProduct(existingProduct.Id);
        _repository.SaveChanges();

        // Assert
        var deletedProduct = _context.Products.Find(existingProduct.Id);
        Assert.Null(deletedProduct);
    }
} 