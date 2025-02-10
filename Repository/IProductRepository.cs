using ProductMicroservice.Models;

namespace ProductMicroservice.Repository;

public interface IProductRepository
{
    IEnumerable<Product> GetProducts();
    Product? GetProductById(int productId);
    void InsertProduct(Product product);
    void DeleteProduct(int productId);
    void UpdateProduct(Product product);
    bool SaveChanges();
    
    // Additional methods for categories
    IEnumerable<Category> GetCategories();
    Category? GetCategoryById(int categoryId);
}
