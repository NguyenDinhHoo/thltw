using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;

namespace AspNetMvcProject.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        return View(await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .ToListAsync());
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (product == null) return NotFound();

        return View(product);
    }

    // GET: Products/Create
    public IActionResult Create()
    {
        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Stock,CategoryId")] Product product, IFormFileCollection images)
    {
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                await SaveImages(product.Id, images);
            }

            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null) return NotFound();
        
        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock,CategoryId")] Product product, IFormFileCollection? newImages, int[]? deleteImages)
    {
        if (id != product.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Xử lý xóa ảnh nếu có
                if (deleteImages != null)
                {
                    foreach (var imageId in deleteImages)
                    {
                        var img = await _context.ProductImages.FindAsync(imageId);
                        if (img != null)
                        {
                            // Có thể xóa file vật lý ở đây nếu muốn
                            _context.ProductImages.Remove(img);
                        }
                    }
                }

                // Xử lý thêm ảnh mới
                if (newImages != null && newImages.Count > 0)
                {
                    await SaveImages(product.Id, newImages);
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) return NotFound();

        return View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product != null)
        {
            // Xóa tất cả ảnh liên quan
            foreach (var img in product.Images)
            {
                _context.ProductImages.Remove(img);
            }
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task SaveImages(int productId, IFormFileCollection images)
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

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
