using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductMicroservice.API.Data;
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
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ProductContext(_options);
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
        Assert.Empty(categories);
    }

    [Fact]
    public void GetCategories_WithExistingData_ReturnsAllCategories()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" }
        };
        _context.Categories.AddRange(categories);
        _context.SaveChanges();

        // Act
        var result = _controller.GetCategories();

        // Assert
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Category>>(result.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public void GetCategory_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test Category" };
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
        var mockContext = new Mock<ProductContext>(_options);
        mockContext.Setup(c => c.Categories).Throws(new Exception("Database error"));
        var controller = new CategoryController(mockContext.Object, _mockLogger.Object);

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
        var mockContext = new Mock<ProductContext>(_options);
        mockContext.Setup(c => c.Categories).Throws(new Exception("Database error"));
        var controller = new CategoryController(mockContext.Object, _mockLogger.Object);

        // Act
        var result = controller.GetCategory(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
} 