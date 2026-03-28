using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.DTOs;
using AspNetMvcProject.Utility;

namespace AspNetMvcProject.Controllers.Api;

[Route("api/orders")]
[ApiController]
public class OrdersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    private ApplicationUser? GetCurrentUser()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users.FirstOrDefault(u => u.Username == username);
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized("Vui lòng đăng nhập.");

        var query = _context.OrderHeaders.AsQueryable();
        
        // If not admin, only see own orders
        if (HttpContext.Session.GetString("Role") != SD.Role_Admin)
        {
            query = query.Where(u => u.ApplicationUserId == user.Id);
        }

        var orders = await query
            .OrderByDescending(u => u.OrderDate)
            .Select(u => new OrderDTO
            {
                Id = u.Id,
                OrderDate = u.OrderDate,
                OrderTotal = u.OrderTotal,
                OrderStatus = u.OrderStatus,
                FullName = u.FullName
            })
            .ToListAsync();

        return Ok(orders);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDTO>> PlaceOrder([FromForm] OrderCreateDTO orderDto)
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized("Vui lòng đăng nhập.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cartItems = await _context.ShoppingCarts
            .Include(u => u.Product)
            .Where(u => u.ApplicationUserId == user.Id)
            .ToListAsync();

        if (!cartItems.Any()) return BadRequest("Giỏ hàng trống.");

        // Calculate total
        double total = 0;
        foreach (var item in cartItems)
        {
            total += (double)(item.Product.Price * item.Count);
        }

        // Create Order Header
        var orderHeader = new OrderHeader
        {
            ApplicationUserId = user.Id,
            OrderDate = DateTime.Now,
            OrderTotal = total,
            OrderStatus = SD.StatusPending,
            PaymentStatus = SD.PaymentStatusPending,
            PhoneNumber = orderDto.PhoneNumber,
            StreetAddress = orderDto.StreetAddress,
            City = orderDto.City,
            FullName = orderDto.FullName
        };

        _context.OrderHeaders.Add(orderHeader);
        await _context.SaveChangesAsync();

        // Create Order Details
        foreach (var item in cartItems)
        {
            var orderDetail = new OrderDetail
            {
                OrderHeaderId = orderHeader.Id,
                ProductId = item.ProductId,
                Count = item.Count,
                Price = (double)item.Product.Price
            };
            _context.OrderDetails.Add(orderDetail);
        }

        // Clear Cart
        _context.ShoppingCarts.RemoveRange(cartItems);
        
        await _context.SaveChangesAsync();
        
        // Clear session cart count
        HttpContext.Session.SetInt32(SD.SessionCart, 0);

        return Ok(new OrderDTO
        {
            Id = orderHeader.Id,
            OrderDate = orderHeader.OrderDate,
            OrderTotal = orderHeader.OrderTotal,
            OrderStatus = orderHeader.OrderStatus,
            FullName = orderHeader.FullName
        });
    }
}
