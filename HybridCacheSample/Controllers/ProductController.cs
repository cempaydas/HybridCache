using System.Text.Json;
using HybridCacheSample.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

namespace HybridCacheSample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cacheL1;
    private readonly IDistributedCache _cacheL2;
    private readonly HybridCache _cache;

    public ProductController(AppDbContext context, IMemoryCache cacheL1, IDistributedCache cacheL2, HybridCache cache)
    {
        _context = context;
        _cacheL1 = cacheL1;
        _cacheL2 = cacheL2;
        _cache = cache;
    }

    // GET: api/Product
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        await Task.Delay(3000);
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }
    [HttpGet("H/{id}")]
    public async Task<ActionResult<Product>> GetProductH(int id)
    {
        var key = $"Product_{id}";
        
        return await _cache.GetOrCreateAsync(key, async (cancellatio)=>{
            await Task.Delay(3000);
            return await _context.Products.FindAsync(id);
        });
    }

    // GET: api/Product/5
    [HttpGet("L1/{id}")]
    public async Task<ActionResult<Product>> GetProductL1(int id)
    {
        var key = $"Product_{id}";
        if (_cacheL1.TryGetValue(key, out Product product))
        {
            return product;
        }
        await Task.Delay(3000);

        product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }
        _cacheL1.Set(key, product);

        return product;
    }
    [HttpGet("L2/{id}")]
    public async Task<ActionResult<Product>> GetProductL2(int id)
    {
        var key = $"Product_{id}";
        var data = await _cacheL2.GetStringAsync(key);
        if (data != null)
        {
            return JsonSerializer.Deserialize<Product>(data);
        }

       
        await Task.Delay(3000);

       var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }
        await _cacheL2.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(product));

        return product;
    }

    // POST: api/Product
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/Product/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
