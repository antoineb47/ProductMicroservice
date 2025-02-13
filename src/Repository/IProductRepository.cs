using ProductMicroservice.Models;

namespace ProductMicroservice.Repository;

public interface IProductRepository
{
    IEnumerable<Product> GetProducts();
    Product? GetProductById(int id);
    Product AddProduct(Product product);
    void DeleteProduct(int id);
    void UpdateProduct(Product product);
    void SaveChanges();
    IEnumerable<Category> GetCategories();
    Category? GetCategoryById(int id);
    Category AddCategory(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(int id);
}
