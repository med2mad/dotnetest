using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetest.Models;
using dotnetest.Data;

namespace dotnetest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.Product)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? NotFound() : order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> PostOrder(Order order)
    {
        // Basic validation
        if (order.Quantity <= 0)
            return BadRequest("Quantity must be positive");

        // Check if product exists
        var product = await _context.Products.FindAsync(order.ProductId);
        if (product == null)
            return BadRequest("Product not found");

        // Ensure it's treated as a new order
        order.Id = 0;
        order.Product = null;
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Return with product details
        order.Product = product;
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest();
        if (order.Quantity <= 0) return BadRequest("Quantity must be positive");

        // Check if product exists
        if (!await _context.Products.AnyAsync(p => p.Id == order.ProductId))
            return BadRequest("Product not found");

        order.Product = null;
        _context.Entry(order).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Orders.Any(o => o.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}