using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.Models;
using ProductMicroservice.Repository;

namespace ProductMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductRepository repository, ILogger<ProductController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        try
        {
            _logger.LogInformation("Getting all products");
            var products = _repository.GetProducts();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all products");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Product> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var product = _repository.GetProductById(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Product> CreateProduct([FromBody] Product product)
    {
        try
        {
            if (product == null)
            {
                _logger.LogWarning("Null product object received");
                return BadRequest("Product object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product creation");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            _repository.InsertProduct(product);
            _repository.SaveChanges();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a new product");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new product");
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            if (product == null || id != product.Id)
            {
                _logger.LogWarning("Invalid product update request for ID: {ProductId}", id);
                return BadRequest("Invalid product data");
            }

            var existingProduct = _repository.GetProductById(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product not found for update, ID: {ProductId}", id);
                return NotFound($"Product with ID {id} not found");
            }

            _logger.LogInformation("Updating product: {ProductId}", id);
            _repository.UpdateProduct(product);
            _repository.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product");
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteProduct(int id)
    {
        try
        {
            var product = _repository.GetProductById(id);
            if (product == null)
            {
                _logger.LogWarning("Product not found for deletion, ID: {ProductId}", id);
                return NotFound($"Product with ID {id} not found");
            }

            _logger.LogInformation("Deleting product: {ProductId}", id);
            _repository.DeleteProduct(id);
            _repository.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product");
        }
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Category>> GetCategories()
    {
        try
        {
            _logger.LogInformation("Getting all categories");
            var categories = _repository.GetCategories();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all categories");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving categories from the database");
        }
    }

    [HttpGet("categories/{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Category> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            var category = _repository.GetCategoryById(id);

            if (category == null)
            {
                _logger.LogWarning("Category with ID: {CategoryId} not found", id);
                return NotFound();
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category with ID: {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving category from the database");
        }
    }
}
