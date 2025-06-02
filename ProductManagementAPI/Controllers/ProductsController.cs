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

        // HTTP PUT Method: Update existing product by replacing it entirely (Unsafe, Idempotent)
        // Updates the entire resource, making it unsafe but idempotent.
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int Id, [FromBody] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                    return BadRequest(ModelState);

                else if (product == null)
                    return BadRequest("Product data cannot be null");

                else if(Id != product.Id)
                    return BadRequest("Product ID mismatch");

                else
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(x => x.Id == Id);

                    if (existingProduct == null)
                        return NotFound($"Product with ID {Id} not found");

                    // Update product product
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }
            //Handle database exception
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while updating the product {ex.InnerException?.Message ?? ex.Message}");
            }
            // Handles any unexpected errors
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product {ex.Message}");
            }
        }

        // HTTP PATCH Method: Partial update of an existing product (Unsafe, Idempotent)
        // Partially updates a resource, unsafe and potentially idempotent depending on how it’s used.
        [HttpPatch("{Id}")]
        public async Task<IActionResult> UpdateProductPrice([FromRoute] int Id, [FromBody] Product product)
        {
            try
            {
                if (Id != product.Id)
                    return BadRequest("Product ID mismatch");

                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (existingProduct == null)
                    return NotFound($"Product with ID: {Id} not found");

                existingProduct.Price = product.Price;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            // Handling any database update exception
            catch(DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while updating the product price {ex.InnerException?.Message ?? ex.Message}");
            }
            // Handling any exception
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product price {ex.Message}");
            }
        }

        // HTTP DELETE Method: Delete an existing product (Unsafe, Idempotent)
        // Removes a resource, unsafe but idempotent because once deleted, subsequent deletes have no effect.
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int Id)
        {
            try
            {

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (product == null)
                    return NotFound($"Product with ID: {Id} not found");

                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                return NoContent();

            }
            // Handle database exception
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product {ex.InnerException?.Message ?? ex.Message}");
            }
            // Handle any unexcepted error
            catch(Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product {ex.Message}");
            }
        }

        // HTTP HEAD Method: Retrieve metadata (headers) for a product without fetching the body
        // Retrieves only headers, safe and idempotent.
        [HttpHead("{Id}")]
        public async Task<IActionResult> HeadProduct([FromRoute] int Id)
        {
            try
            {
                var productExists = await _context.Products.AnyAsync(p => p.Id == Id);

                if (!productExists)
                    return NotFound(); // Returns 404 Not Found if product does not exist

                // Add desired headers
                // Set Content-Type header to indicate the media type of the resource (JSON in this case)
                Response.Headers.Append("Content-Type", "application/json");

                // Set Content-Length header to indicate the size of the response body (though body is not returned in HEAD request)
                var contentLength = System.Text.Json.JsonSerializer.Serialize(productExists).Length;

                Response.Headers.Append("Content-Length", contentLength.ToString());

                // Add a custom header (e.g., "X-Custom-Header")
                Response.Headers.Append("X-Content-Header", "CustomHeaderValue");

                // Return status 200 OK (with headers only)
                return Ok();

            }
            // Handle any unexpected errors
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving product metadata {ex.Message}");
            }
        }

        // HTTP OPTIONS Method: List allowed methods for this controller (Safe)
        // Safe and idempotent, useful for preflight requests or discovering allowed methods.
        [HttpOptions]
        public IActionResult GetOptions()
        {
            try
            {
                // Returns the list of supported HTTP methods
                Response.Headers.Append("Allow", "GET, POST, PUT, PATCH, DELETE, OPTIONS, HEAD");

                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving options");
            }
        }

    }
}
