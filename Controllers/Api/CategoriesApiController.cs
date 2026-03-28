using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.DTOs;

namespace AspNetMvcProject.Controllers.Api;

[Route("api/categories")]
[ApiController]
public class CategoriesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name
        });
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> CreateCategory([FromForm] CategoryDTO categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = new Category
        {
            Name = categoryDto.Name
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        categoryDto.Id = category.Id;

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
    }

    // PUT: api/categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromForm] CategoryDTO categoryDto)
    {
        if (id != categoryDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = categoryDto.Name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Categories.Any(c => c.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            return BadRequest("Cannot delete category with associated products.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
