using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.ViewModels;
using AspNetMvcProject.Utility;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.Username == username);
        
        ShoppingCartVM = new()
        {
            ShoppingCartList = _db.ShoppingCarts.Include(u => u.Product).ThenInclude(u => u.Images)
                .Where(u => u.ApplicationUserId == user.Id),
            OrderHeader = new()
        };

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = (double)cart.Product.Price;
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    public IActionResult Summary()
    {
        var username = HttpContext.Session.GetString("Username");
        var user = _db.Users.FirstOrDefault(u => u.Username == username);

        ShoppingCartVM = new()
        {
            ShoppingCartList = _db.ShoppingCarts.Include(u => u.Product)
                .Where(u => u.ApplicationUserId == user.Id),
            OrderHeader = new()
        };

        ShoppingCartVM.OrderHeader.ApplicationUser = user;
        ShoppingCartVM.OrderHeader.FullName = user.FullName ?? user.Username;
        ShoppingCartVM.OrderHeader.PhoneNumber = "0123456789"; // Placeholder
        ShoppingCartVM.OrderHeader.StreetAddress = user.Address ?? "";
        ShoppingCartVM.OrderHeader.City = "Hà Nội"; // Placeholder

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = (double)cart.Product.Price;
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
	public IActionResult SummaryPOST()
	{
		var username = HttpContext.Session.GetString("Username");
		var user = _db.Users.FirstOrDefault(u => u.Username == username);

		ShoppingCartVM.ShoppingCartList = _db.ShoppingCarts.Include(u => u.Product)
			.Where(u => u.ApplicationUserId == user.Id);

		ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
		ShoppingCartVM.OrderHeader.ApplicationUserId = user.Id;

		foreach (var cart in ShoppingCartVM.ShoppingCartList)
		{
			cart.Price = (double)cart.Product.Price;
			ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
		}

		ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
		ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        ShoppingCartVM.OrderHeader.PaymentDate = System.DateTime.Now;

		_db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
		_db.SaveChanges();

		foreach (var cart in ShoppingCartVM.ShoppingCartList)
		{
			OrderDetail orderDetail = new()
			{
				ProductId = cart.ProductId,
				OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
				Price = cart.Price,
				Count = cart.Count
			};
			_db.OrderDetails.Add(orderDetail);
		}
		_db.SaveChanges();

		// Clear cart and session
		List<ShoppingCart> shoppingCarts = _db.ShoppingCarts.Where(u => u.ApplicationUserId == user.Id).ToList();
		_db.ShoppingCarts.RemoveRange(shoppingCarts);
		_db.SaveChanges();
		HttpContext.Session.SetInt32(SD.SessionCart, 0);
        TempData["success"] = "Thanh toán thành công!";


		return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
	}

    public IActionResult OrderConfirmation(int id)
    {
        return View(id);
    }

    public IActionResult Plus(int cartId)
    {
        var cartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
        cartFromDb.Count += 1;
        _db.ShoppingCarts.Update(cartFromDb);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var cartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
        if (cartFromDb.Count <= 1)
        {
            _db.ShoppingCarts.Remove(cartFromDb);
            var count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
        }
        else
        {
            cartFromDb.Count -= 1;
            _db.ShoppingCarts.Update(cartFromDb);
        }
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int cartId)
    {
        var cartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
        _db.ShoppingCarts.Remove(cartFromDb);
        _db.SaveChanges();
        var count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count();
        HttpContext.Session.SetInt32(SD.SessionCart, count);

        return RedirectToAction(nameof(Index));
    }
}
