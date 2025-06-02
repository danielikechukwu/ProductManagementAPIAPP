using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Models;

namespace ProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }

        // HTTP GET Method: Retrieve all products (Safe, Idempotent)
        // Safely retrieves resources without altering the server state. It's idempotent and safe.
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _context.Products
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving products: {ex.Message}");
            }
        }

        // HTTP GET Method: Retrieve product by ID (Safe, Idempotent)
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetProductById([FromRoute] int Id)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (product == null)
                    return NotFound($"Product with ID: {Id} not found");

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the product {ex.Message}");
            }
        }

        // HTTP POST Method: Create a new product (Unsafe, Non-Idempotent)
        // Creates new resources, which makes it unsafe and non-idempotent.
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else if (product == null)
                {
                    return BadRequest("Product data cannot be null");
                }
                else
                {
                    await _context.Products.AddAsync(product);

                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetProductById), new {id = product.Id}, product);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating product {ex.Message}");
            }
        }
    }
}
