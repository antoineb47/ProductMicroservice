using Microsoft.EntityFrameworkCore;
using ProductMicroservice.API.Data;
using ProductMicroservice.Models;

namespace ProductMicroservice.Repository;

/// <summary>
/// Implémentation du repository pour la gestion des produits et des catégories
/// Gère les opérations de base de données en utilisant Entity Framework Core
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductContext _context;
    private readonly ILogger<ProductRepository> _logger;

    /// <summary>
    /// Constructeur du repository
    /// </summary>
    /// <param name="context">Le contexte de base de données</param>
    /// <param name="logger">Le logger pour tracer les opérations</param>
    public ProductRepository(ProductContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les produits disponibles
    /// </summary>
    /// <returns>Une collection de tous les produits avec leurs catégories associées</returns>
    public IEnumerable<Product> GetProducts()
    {
        try
        {
            return _context.Products.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les produits");
            throw;
        }
    }

    /// <summary>
    /// Récupère un produit spécifique par son identifiant
    /// </summary>
    /// <param name="productId">L'identifiant du produit à récupérer</param>
    /// <returns>Le produit correspondant à l'identifiant ou null si non trouvé</returns>
    public Product? GetProductById(int productId)
    {
        try
        {
            return _context.Products.FirstOrDefault(p => p.Id == productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du produit avec l'ID: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Ajoute un nouveau produit dans la base de données
    /// </summary>
    /// <param name="product">Le produit à ajouter</param>
    /// <returns>Le produit ajouté</returns>
    public Product AddProduct(Product product)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(product);
            var result = _context.Products.Add(product);
            _context.SaveChanges();
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du produit: {ProductName}", product?.Name);
            throw;
        }
    }

    /// <summary>
    /// Supprime un produit existant
    /// </summary>
    /// <param name="productId">L'identifiant du produit à supprimer</param>
    public void DeleteProduct(int productId)
    {
        try
        {
            var product = _context.Products.Find(productId) 
                ?? throw new KeyNotFoundException($"Produit avec l'ID {productId} non trouvé");
            
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du produit avec l'ID: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Met à jour les informations d'un produit existant
    /// </summary>
    /// <param name="product">Le produit avec les nouvelles informations</param>
    public void UpdateProduct(Product product)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(product);
            
            var existingProduct = _context.Products.Find(product.Id);
            if (existingProduct != null)
            {
                _context.Entry(existingProduct).CurrentValues.SetValues(product);
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du produit: {ProductId}", product?.Id);
            throw;
        }
    }

    /// <summary>
    /// Sauvegarde les modifications dans la base de données
    /// </summary>
    /// <returns>True si les modifications ont été sauvegardées avec succès, False sinon</returns>
    public bool SaveChanges()
    {
        try
        {
            return _context.SaveChanges() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde des modifications dans la base de données");
            throw;
        }
    }

    /// <summary>
    /// Récupère toutes les catégories disponibles
    /// </summary>
    /// <returns>Une collection de toutes les catégories avec leurs produits associés</returns>
    public IEnumerable<Category> GetCategories()
    {
        try
        {
            return _context.Categories.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de toutes les catégories");
            throw;
        }
    }

    /// <summary>
    /// Récupère une catégorie spécifique par son identifiant
    /// </summary>
    /// <param name="categoryId">L'identifiant de la catégorie à récupérer</param>
    /// <returns>La catégorie correspondante à l'identifiant ou null si non trouvée</returns>
    public Category? GetCategoryById(int categoryId)
    {
        try
        {
            return _context.Categories.FirstOrDefault(c => c.Id == categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la catégorie avec l'ID: {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Ajoute une nouvelle catégorie dans la base de données
    /// </summary>
    /// <param name="category">La catégorie à ajouter</param>
    /// <returns>La catégorie ajoutée</returns>
    public Category AddCategory(Category category)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(category);
            var result = _context.Categories.Add(category);
            _context.SaveChanges();
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout de la catégorie: {CategoryName}", category?.Name);
            throw;
        }
    }

    /// <summary>
    /// Met à jour une catégorie existante
    /// </summary>
    /// <param name="category">La catégorie avec les nouvelles informations</param>
    public void UpdateCategory(Category category)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(category);
            
            var existingCategory = _context.Categories.Find(category.Id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Catégorie avec l'ID {category.Id} non trouvée");
            }

            _context.Entry(existingCategory).CurrentValues.SetValues(category);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la catégorie: {CategoryId}", category?.Id);
            throw;
        }
    }

    /// <summary>
    /// Supprime une catégorie existante
    /// </summary>
    /// <param name="categoryId">L'identifiant de la catégorie à supprimer</param>
    public void DeleteCategory(int categoryId)
    {
        try
        {
            var category = _context.Categories.Find(categoryId) 
                ?? throw new KeyNotFoundException($"Catégorie avec l'ID {categoryId} non trouvée");
            
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la catégorie avec l'ID: {CategoryId}", categoryId);
            throw;
        }
    }
}
