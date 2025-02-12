namespace ProductMicroservice.Models;

/// <summary>
/// Représente une catégorie de produits
/// </summary>
public class Category
{
    /// <summary>
    /// Identifiant unique de la catégorie
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom de la catégorie
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description de la catégorie
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
