using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Data;
using ProductMicroservice.Models;

namespace ProductMicroservice.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ProductContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ProductContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IEnumerable<Product> GetProducts()
    {
        try
        {
            return _context.Products.Include(p => p.Category).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all products");
            throw;
        }
    }

    public Product? GetProductById(int productId)
    {
        try
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product with ID: {ProductId}", productId);
            throw;
        }
    }

    public void InsertProduct(Product product)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(product);
            _context.Products.Add(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while inserting product: {ProductName}", product?.Name);
            throw;
        }
    }

    public void DeleteProduct(int productId)
    {
        try
        {
            var product = _context.Products.Find(productId) 
                ?? throw new KeyNotFoundException($"Product with ID {productId} not found");
            
            _context.Products.Remove(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID: {ProductId}", productId);
            throw;
        }
    }

    public void UpdateProduct(Product product)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(product);
            _context.Entry(product).State = EntityState.Modified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product: {ProductId}", product?.Id);
            throw;
        }
    }

    public bool SaveChanges()
    {
        try
        {
            return _context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to the database");
            throw;
        }
    }

    public IEnumerable<Category> GetCategories()
    {
        try
        {
            return _context.Categories.Include(c => c.Products).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all categories");
            throw;
        }
    }

    public Category? GetCategoryById(int categoryId)
    {
        try
        {
            return _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving category with ID: {CategoryId}", categoryId);
            throw;
        }
    }
}
