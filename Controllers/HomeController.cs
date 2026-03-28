using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AspNetMvcProject.Data.ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, AspNetMvcProject.Data.ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var productList = _db.Products.Include(u => u.Category).Include(u => u.Images).ToList();
        return View(productList);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart cartObj = new()
        {
            Count = 1,
            ProductId = productId,
            Product = _db.Products.Include(u => u.Category).Include(u => u.Images).FirstOrDefault(u => u.Id == productId)
        };

        if (cartObj.Product == null)
        {
            return NotFound();
        }

        return View(cartObj);
    }

    [HttpPost]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        // Get user from session
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        shoppingCart.ApplicationUserId = user.Id;

        // Check if cart item already exists for this user and product
        ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(
            u => u.ApplicationUserId == user.Id && u.ProductId == shoppingCart.ProductId);

        if (cartFromDb == null)
        {
            // Add new cart item
            _db.ShoppingCarts.Add(shoppingCart);
        }
        else
        {
            // Update existing cart item
            cartFromDb.Count += shoppingCart.Count;
            _db.ShoppingCarts.Update(cartFromDb);
        }
        _db.SaveChanges();

        // Update session cart count (optional but good for UI)
        var count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == user.Id).Count();
        HttpContext.Session.SetInt32(Utility.SD.SessionCart, count);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult AddToCartQuick(int productId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Người dùng không tồn tại!" });
            }
            return RedirectToAction("Login", "Account");
        }

        ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(
            u => u.ApplicationUserId == user.Id && u.ProductId == productId);

        if (cartFromDb == null)
        {
            ShoppingCart shoppingCart = new()
            {
                ProductId = productId,
                ApplicationUserId = user.Id,
                Count = 1
            };
            _db.ShoppingCarts.Add(shoppingCart);
        }
        else
        {
            cartFromDb.Count += 1;
            _db.ShoppingCarts.Update(cartFromDb);
        }
        _db.SaveChanges();

        var count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == user.Id).Count();
        HttpContext.Session.SetInt32(Utility.SD.SessionCart, count);

        TempData["success"] = "Đã thêm vào giỏ hàng!";

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", count = count });
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
