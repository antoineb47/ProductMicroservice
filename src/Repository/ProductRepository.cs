using Microsoft.EntityFrameworkCore;
using ProductMicroservice.API.Data;
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
            _logger.LogInformation("Récupération de tous les produits");
            return _context.Products.Include(p => p.Category).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des produits");
            throw;
        }
    }

    public Product? GetProductById(int id)
    {
        try
        {
            _logger.LogInformation("Récupération du produit avec l'ID: {ProductId}", id);
            return _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du produit avec l'ID: {ProductId}", id);
            throw;
        }
    }

    public Product AddProduct(Product product)
    {
        try
        {
            _logger.LogInformation("Ajout d'un nouveau produit: {ProductName}", product.Name);
            _context.Products.Add(product);
            SaveChanges();
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du produit: {ProductName}", product.Name);
            throw;
        }
    }

    public void DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Suppression du produit: {ProductId}", id);
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du produit: {ProductId}", id);
            throw;
        }
    }

    public void UpdateProduct(Product product)
    {
        try
        {
            _logger.LogInformation("Mise à jour du produit: {ProductId}", product.Id);
            _context.Entry(product).State = EntityState.Modified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du produit: {ProductId}", product.Id);
            throw;
        }
    }

    public void SaveChanges()
    {
        try
        {
            _logger.LogInformation("Sauvegarde des modifications dans la base de données");
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde des modifications");
            throw;
        }
    }

    public IEnumerable<Category> GetCategories()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les catégories");
            return _context.Categories.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des catégories");
            throw;
        }
    }

    public Category? GetCategoryById(int id)
    {
        try
        {
            _logger.LogInformation("Récupération de la catégorie avec l'ID: {CategoryId}", id);
            return _context.Categories.Find(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la catégorie avec l'ID: {CategoryId}", id);
            throw;
        }
    }

    public Category AddCategory(Category category)
    {
        try
        {
            _logger.LogInformation("Ajout d'une nouvelle catégorie: {CategoryName}", category.Name);
            _context.Categories.Add(category);
            SaveChanges();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout de la catégorie: {CategoryName}", category.Name);
            throw;
        }
    }

    public void UpdateCategory(Category category)
    {
        try
        {
            _logger.LogInformation("Mise à jour de la catégorie: {CategoryId}", category.Id);
            _context.Entry(category).State = EntityState.Modified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la catégorie: {CategoryId}", category.Id);
            throw;
        }
    }

    public void DeleteCategory(int id)
    {
        try
        {
            _logger.LogInformation("Suppression de la catégorie: {CategoryId}", id);
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la catégorie: {CategoryId}", id);
            throw;
        }
    }
}
