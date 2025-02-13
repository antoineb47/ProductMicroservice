using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Data;
using ProductMicroservice.Models;
using Microsoft.Extensions.Logging;

namespace ProductMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ProductContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Category
        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            try
            {
                _logger.LogInformation("Récupération de toutes les catégories");
                return _context.Categories.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des catégories");
                return StatusCode(500, "Erreur lors de la récupération des catégories");
            }
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public ActionResult<Category> GetCategory(int id)
        {
            try
            {
                _logger.LogInformation("Récupération de la catégorie avec l'ID: {CategoryId}", id);
                var category = _context.Categories.Find(id);

                if (category == null)
                {
                    _logger.LogWarning("Catégorie avec l'ID: {CategoryId} non trouvée", id);
                    return NotFound();
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la catégorie avec l'ID: {CategoryId}", id);
                return StatusCode(500, "Erreur lors de la récupération de la catégorie");
            }
        }
    }
} 