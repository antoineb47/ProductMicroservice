using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductMicroservice.Controllers;
using ProductMicroservice.Models;
using ProductMicroservice.Repository;
using Xunit;

namespace ProductMicroservice.Tests;

public class ProductControllerTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetProducts_ReturnsOkResult_WithProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = 1, Name = "Test Product 1", Price = 10.99m },
            new() { Id = 2, Name = "Test Product 2", Price = 20.99m }
        };
        _mockRepo.Setup(repo => repo.GetProducts()).Returns(expectedProducts);

        // Act
        var result = _controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public void GetProduct_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var expectedProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m };
        _mockRepo.Setup(repo => repo.GetProductById(1)).Returns(expectedProduct);

        // Act
        var result = _controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(expectedProduct.Id, product.Id);
    }

    [Fact]
    public void GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetProductById(999)).Returns((Product)null);

        // Act
        var result = _controller.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void CreateProduct_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new Product { Name = "New Product", Price = 15.99m };
        _mockRepo.Setup(repo => repo.InsertProduct(It.IsAny<Product>()));
        _mockRepo.Setup(repo => repo.SaveChanges()).Returns(true);

        // Act
        var result = _controller.CreateProduct(newProduct);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal(newProduct.Name, product.Name);
    }

    [Fact]
    public void UpdateProduct_WithValidProduct_ReturnsNoContent()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, Name = "Existing Product", Price = 10.99m };
        _mockRepo.Setup(repo => repo.GetProductById(1)).Returns(existingProduct);
        _mockRepo.Setup(repo => repo.UpdateProduct(It.IsAny<Product>()));
        _mockRepo.Setup(repo => repo.SaveChanges()).Returns(true);

        // Act
        var result = _controller.UpdateProduct(1, existingProduct);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void UpdateProduct_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10.99m };

        // Act
        var result = _controller.UpdateProduct(2, product);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m };
        _mockRepo.Setup(repo => repo.GetProductById(1)).Returns(existingProduct);
        _mockRepo.Setup(repo => repo.DeleteProduct(1));
        _mockRepo.Setup(repo => repo.SaveChanges()).Returns(true);

        // Act
        var result = _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetProductById(999)).Returns((Product)null);

        // Act
        var result = _controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetCategories_ReturnsOkResult_WithCategories()
    {
        // Arrange
        var expectedCategories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Clothes" }
        };
        _mockRepo.Setup(repo => repo.GetCategories()).Returns(expectedCategories);

        // Act
        var result = _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(2, categories.Count());
    }

    [Fact]
    public void GetCategory_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var expectedCategory = new Category { Id = 1, Name = "Electronics" };
        _mockRepo.Setup(repo => repo.GetCategoryById(1)).Returns(expectedCategory);

        // Act
        var result = _controller.GetCategory(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<Category>(okResult.Value);
        Assert.Equal(expectedCategory.Id, category.Id);
    }
}
