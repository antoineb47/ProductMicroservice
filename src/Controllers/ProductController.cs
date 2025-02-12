using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.Models;
using ProductMicroservice.Repository;

namespace ProductMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductRepository productRepository, ILogger<ProductController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        try
        {
            _logger.LogInformation("Récupération de tous les produits");
            var products = _productRepository.GetProducts();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des produits");
            return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de la récupération des produits");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Récupération du produit avec l'ID: {ProductId}", id);
            var product = _productRepository.GetProductById(id);

            if (product == null)
            {
                _logger.LogWarning("Produit avec l'ID: {ProductId} non trouvé", id);
                return NotFound($"Produit avec l'ID {id} non trouvé");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du produit avec l'ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de la récupération du produit");
        }
    }

    [HttpPost]
    public ActionResult<Product> PostProduct(Product product)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Données du produit invalides");
                return BadRequest(ModelState);
            }

            var category = _productRepository.GetCategoryById(product.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("La catégorie spécifiée n'existe pas");
                return BadRequest("La catégorie spécifiée n'existe pas");
            }

            _logger.LogInformation("Création d'un nouveau produit: {ProductName}", product.Name);
            var addedProduct = _productRepository.AddProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = addedProduct.Id }, addedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du produit");
            return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de la création du produit");
        }
    }

    [HttpPut("{id}")]
    public IActionResult PutProduct(int id, Product product)
    {
        try
        {
            if (id != product.Id)
            {
                _logger.LogWarning("Données de mise à jour invalides pour l'ID: {ProductId}", id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Données de mise à jour invalides");
                return BadRequest(ModelState);
            }

            var category = _productRepository.GetCategoryById(product.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("La catégorie spécifiée n'existe pas");
                return BadRequest("La catégorie spécifiée n'existe pas");
            }

            _logger.LogInformation("Mise à jour du produit: {ProductId}", id);
            _productRepository.UpdateProduct(product);
            _productRepository.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du produit avec l'ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de la mise à jour du produit");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        try
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                _logger.LogWarning("Produit non trouvé pour la suppression, ID: {ProductId}", id);
                return NotFound($"Produit avec l'ID {id} non trouvé");
            }

            _logger.LogInformation("Suppression du produit: {ProductId}", id);
            _productRepository.DeleteProduct(id);
            _productRepository.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du produit avec l'ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de la suppression du produit");
        }
    }
}
