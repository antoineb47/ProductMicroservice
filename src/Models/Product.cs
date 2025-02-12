namespace ProductMicroservice.Models;

/// <summary>
/// Représente un produit
/// </summary>
public class Product
{
    /// <summary>
    /// Identifiant unique du produit
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom du produit
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description du produit
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Prix du produit
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Identifiant de la catégorie
    /// </summary>
    public int CategoryId { get; set; }
}
