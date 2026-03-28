using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.DTOs;

namespace AspNetMvcProject.Controllers.Api;

[Route("api/cart")]
[ApiController]
public class CartApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    private ApplicationUser? GetCurrentUser()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users.FirstOrDefault(u => u.Username == username);
    }

    // GET: api/cart
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDTO>>> GetCart()
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized("Vui lòng đăng nhập.");

        var cartItems = await _context.ShoppingCarts
            .Include(u => u.Product)
            .ThenInclude(p => p.Images)
            .Where(u => u.ApplicationUserId == user.Id)
            .Select(u => new CartItemDTO
            {
                Id = u.Id,
                ProductId = u.ProductId,
                ProductName = u.Product.Name,
                ProductPrice = u.Product.Price,
                Count = u.Count,
                ProductImageUrl = u.Product.Images.FirstOrDefault() != null ? u.Product.Images.FirstOrDefault().ImageUrl : null
            })
            .ToListAsync();

        return Ok(cartItems);
    }

    // POST: api/cart
    [HttpPost]
    public async Task<ActionResult<CartItemDTO>> UpsertCart([FromForm] CartUpsertDTO cartDto)
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized("Vui lòng đăng nhập.");

        var product = await _context.Products.FindAsync(cartDto.ProductId);
        if (product == null) return BadRequest("Sản phẩm không tồn tại.");

        var cartFromDb = await _context.ShoppingCarts.FirstOrDefaultAsync(
            u => u.ApplicationUserId == user.Id && u.ProductId == cartDto.ProductId);

        if (cartFromDb == null)
        {
            // Add new
            cartFromDb = new ShoppingCart
            {
                ApplicationUserId = user.Id,
                ProductId = cartDto.ProductId,
                Count = cartDto.Count
            };
            _context.ShoppingCarts.Add(cartFromDb);
        }
        else
        {
            // Update quantity
            cartFromDb.Count += cartDto.Count;
        }

        await _context.SaveChangesAsync();

        // Return updated item info
        var result = new CartItemDTO
        {
            Id = cartFromDb.Id,
            ProductId = cartFromDb.ProductId,
            ProductName = product.Name,
            ProductPrice = product.Price,
            Count = cartFromDb.Count
        };

        return Ok(result);
    }

    // PUT: api/cart/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCart(int id, [FromForm] int count)
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized("Vui lòng đăng nhập.");

        var cartItem = await _context.ShoppingCarts.FirstOrDefaultAsync(u => u.Id == id && u.ApplicationUserId == user.Id);
        if (cartItem == null) return NotFound("Không tìm thấy mục trong giỏ hàng.");

        if (count <= 0)
        {
            _context.ShoppingCarts.Remove(cartItem);
        }
        else
        {
            cartItem.Count = count;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/cart/5
}
