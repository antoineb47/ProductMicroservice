using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;


    public ProductControllerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetProducts_ReturnsOkResult()
    {
        // Arrange
        var testProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 10.99m, CategoryId = 1 },
            new Product { Id = 2, Name = "Test Product 2", Price = 20.99m, CategoryId = 1 }
        };
        _mockRepository.Setup(repo => repo.GetProducts()).Returns(testProducts);

        // Act
        var result = _controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<Product>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public void GetProduct_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CategoryId = 1 };
        _mockRepository.Setup(repo => repo.GetProductById(1)).Returns(testProduct);

        // Act
        var result = _controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<Product>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public void GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        Product? nullProduct = null;
        _mockRepository.Setup(repo => repo.GetProductById(It.IsAny<int>())).Returns(nullProduct);

        // Act
        var result = _controller.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void PostProduct_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CategoryId = 1 };
        var testCategory = new Category { Id = 1, Name = "Test Category" };
        _mockRepository.Setup(repo => repo.GetCategoryById(1)).Returns(testCategory);
        _mockRepository.Setup(repo => repo.AddProduct(It.IsAny<Product>())).Returns(testProduct);

        // Act
        var result = _controller.PostProduct(testProduct);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<Product>(createdAtActionResult.Value);
        Assert.Equal(testProduct.Id, returnValue.Id);
    }

    [Fact]
    public void PostProduct_WithInvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CategoryId = 999 };
        Category? nullCategory = null;
        _mockRepository.Setup(repo => repo.GetCategoryById(999)).Returns(nullCategory);

        // Act
        var result = _controller.PostProduct(testProduct);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void PutProduct_WithValidProduct_ReturnsNoContent()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Updated Product", Price = 15.99m, CategoryId = 1 };
        var testCategory = new Category { Id = 1, Name = "Test Category" };
        _mockRepository.Setup(repo => repo.GetCategoryById(1)).Returns(testCategory);

        // Act
        var result = _controller.PutProduct(1, testProduct);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Once);
        _mockRepository.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public void PutProduct_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var testProduct = new Product { Id = 2, Name = "Test Product", Price = 10.99m, CategoryId = 1 };

        // Act
        var result = _controller.PutProduct(1, testProduct);

        // Assert
        Assert.IsType<BadRequestResult>(result);
        _mockRepository.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void PutProduct_WithInvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CategoryId = 999 };
        Category? nullCategory = null;
        _mockRepository.Setup(repo => repo.GetCategoryById(999)).Returns(nullCategory);

        // Act
        var result = _controller.PutProduct(1, testProduct);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CategoryId = 1 };
        _mockRepository.Setup(repo => repo.GetProductById(1)).Returns(testProduct);

        // Act
        var result = _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(repo => repo.DeleteProduct(1), Times.Once);
        _mockRepository.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public void DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        Product? nullProduct = null;
        _mockRepository.Setup(repo => repo.GetProductById(999)).Returns(nullProduct);

        // Act
        var result = _controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _mockRepository.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
    }
} 