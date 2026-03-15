using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetest.Models;
using dotnetest.Data;

namespace dotnetest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product == null ? NotFound() : product;
    }

    [HttpGet("{id}/orders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetProductOrders(int id)
    {
        var product = await _context.Products
            .Include(p => p.Orders)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? NotFound() : product.Orders.ToList();
    }

    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0)
            return BadRequest("Invalid product data");

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest();

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(p => p.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}