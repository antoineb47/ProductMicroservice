namespace ProductMicroservice.Models;

/// <summary>
/// Représente un produit
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
}
