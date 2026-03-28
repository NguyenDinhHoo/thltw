using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.DTOs;

namespace AspNetMvcProject.Controllers.Api;

[Route("api/products")]
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductsApiController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(string? search)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        var products = await query
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                Images = p.Images.Select(i => new ProductImageDTO { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var productDto = new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Images = product.Images.Select(i => new ProductImageDTO { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
        };

        return Ok(productDto);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDTO>> CreateProduct([FromForm] ProductCreateDTO productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Stock = productDto.Stock,
            CategoryId = productDto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Handle Images
        if (productDto.Images != null && productDto.Images.Count > 0)
        {
            await SaveImagesToDisk(product.Id, productDto.Images);
        }

        // Map back to DTO
        var resultDto = new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, resultDto);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDTO productDto)
    {
        if (id != productDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.Stock = productDto.Stock;
        product.Stock = productDto.Stock;
        product.CategoryId = productDto.CategoryId;

        // Handle Image Deletion
        if (productDto.DeleteImageIds != null && productDto.DeleteImageIds.Count > 0)
        {
            foreach (var imageId in productDto.DeleteImageIds)
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);
                if (image != null)
                {
                    // Physically delete
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _context.ProductImages.Remove(image);
                }
            }
        }

        // Handle New Images
        if (productDto.Images != null && productDto.Images.Count > 0)
        {
            await SaveImagesToDisk(product.Id, productDto.Images);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(p => p.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null)
        {
            return NotFound();
        }

        // Remove images related to product
        if (product.Images != null)
        {
            _context.ProductImages.RemoveRange(product.Images);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task SaveImagesToDisk(int productId, List<IFormFile> images)
    {
        string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "products");
        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

        foreach (var file in images)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _context.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                ImageUrl = "/uploads/products/" + fileName
            });
        }
        await _context.SaveChangesAsync();
    }
}
