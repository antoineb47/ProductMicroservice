using ProductMicroservice.Models;

namespace ProductMicroservice.Repository;

/// <summary>
/// Interface définissant les opérations de base pour la gestion des produits et des catégories
/// Fournit les méthodes CRUD pour les produits et les méthodes de lecture pour les catégories
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Récupère tous les produits disponibles
    /// </summary>
    /// <returns>Une collection de tous les produits avec leurs catégories associées</returns>
    IEnumerable<Product> GetProducts();

    /// <summary>
    /// Récupère un produit spécifique par son identifiant
    /// </summary>
    /// <param name="productId">L'identifiant du produit à récupérer</param>
    /// <returns>Le produit correspondant à l'identifiant ou null si non trouvé</returns>
    Product? GetProductById(int productId);

    /// <summary>
    /// Ajoute un nouveau produit dans la base de données
    /// </summary>
    /// <param name="product">Le produit à ajouter</param>
    Product AddProduct(Product product);

    /// <summary>
    /// Supprime un produit existant
    /// </summary>
    /// <param name="productId">L'identifiant du produit à supprimer</param>
    void DeleteProduct(int productId);

    /// <summary>
    /// Met à jour les informations d'un produit existant
    /// </summary>
    /// <param name="product">Le produit avec les nouvelles informations</param>
    void UpdateProduct(Product product);

    /// <summary>
    /// Sauvegarde les modifications dans la base de données
    /// </summary>
    /// <returns>True si les modifications ont été sauvegardées avec succès, False sinon</returns>
    bool SaveChanges();

    /// <summary>
    /// Récupère toutes les catégories disponibles
    /// </summary>
    /// <returns>Une collection de toutes les catégories avec leurs produits associés</returns>
    IEnumerable<Category> GetCategories();

    /// <summary>
    /// Récupère une catégorie spécifique par son identifiant
    /// </summary>
    /// <param name="categoryId">L'identifiant de la catégorie à récupérer</param>
    /// <returns>La catégorie correspondante à l'identifiant ou null si non trouvée</returns>
    Category? GetCategoryById(int categoryId);

    /// <summary>
    /// Ajoute une nouvelle catégorie dans la base de données
    /// </summary>
    /// <param name="category">La catégorie à ajouter</param>
    Category AddCategory(Category category);

    /// <summary>
    /// Met à jour les informations d'une catégorie existante
    /// </summary>
    /// <param name="category">La catégorie avec les nouvelles informations</param>
    void UpdateCategory(Category category);

    /// <summary>
    /// Supprime une catégorie existante
    /// </summary>
    /// <param name="categoryId">L'identifiant de la catégorie à supprimer</param>
    void DeleteCategory(int categoryId);
}
