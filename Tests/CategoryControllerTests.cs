using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductMicroservice.Data;
using ProductMicroservice.Controllers;
using ProductMicroservice.Models;
using Xunit;

namespace ProductMicroservice.Tests;

public class CategoryControllerTests : IDisposable
{
    private readonly Mock<ILogger<CategoryController>> _mockLogger;
    private readonly ProductContext _context;
    private readonly CategoryController _controller;
    private readonly DbContextOptions<ProductContext> _options;

    public CategoryControllerTests()
    {
        _options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductContext(_options);
        
        // Seed the database with initial categories
        _context.Categories.AddRange(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic Items" },
            new Category { Id = 2, Name = "Clothes", Description = "Dresses" },
            new Category { Id = 3, Name = "Grocery", Description = "Grocery Items" }
        );
        _context.SaveChanges();
        
        _mockLogger = new Mock<ILogger<CategoryController>>();
        _controller = new CategoryController(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void GetCategories_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetCategories();

        // Assert
        Assert.IsType<ActionResult<IEnumerable<Category>>>(result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(result.Value);
        Assert.Equal(3, categories.Count());
    }

    [Fact]
    public void GetCategories_WithExistingData_ReturnsAllCategories()
    {
        // Arrange
        var newCategories = new[]
        {
            new Category { Id = 4, Name = "Category 4" },
            new Category { Id = 5, Name = "Category 5" }
        };
        _context.Categories.AddRange(newCategories);
        _context.SaveChanges();

        // Act
        var result = _controller.GetCategories();

        // Assert
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Category>>(result.Value);
        Assert.Equal(5, returnValue.Count());
    }

    [Fact]
    public void GetCategory_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var category = new Category { Id = 4, Name = "Test Category" };
        _context.Categories.Add(category);
        _context.SaveChanges();

        // Act
        var result = _controller.GetCategory(category.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Category>>(result);
        var returnValue = Assert.IsType<Category>(actionResult.Value);
        Assert.Equal(category.Id, returnValue.Id);
        Assert.Equal(category.Name, returnValue.Name);
    }

    [Fact]
    public void GetCategory_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = _controller.GetCategory(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetCategories_WhenExceptionOccurs_ReturnsInternalError()
    {
        // Arrange
        var exceptionContext = new ExceptionProductContext(_options);
        var controller = new CategoryController(exceptionContext, _mockLogger.Object);

        // Act
        var result = controller.GetCategories();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public void GetCategory_WhenExceptionOccurs_ReturnsInternalError()
    {
        // Arrange
        var exceptionContext = new ExceptionProductContext(_options);
        var controller = new CategoryController(exceptionContext, _mockLogger.Object);

        // Act
        var result = controller.GetCategory(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}

// Ajout d'une classe locale pour simuler une exception lors de l'accès à Categories
public class ExceptionProductContext : ProductMicroservice.Data.ProductContext
{
    public ExceptionProductContext(Microsoft.EntityFrameworkCore.DbContextOptions<ProductMicroservice.Data.ProductContext> options) : base(options) { }
    public override Microsoft.EntityFrameworkCore.DbSet<ProductMicroservice.Models.Category> Categories => throw new Exception("Database error");
} 