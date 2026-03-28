using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<ApplicationUser>();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (ModelState.IsValid)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (userExists)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            var user = new ApplicationUser
            {
                Username = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                Age = model.Age,
                Role = Utility.SD.Role_Customer
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role ?? "User");
                    
                    // Set cart count in session
                    var cartCount = _context.ShoppingCarts.Where(u => u.ApplicationUserId == user.Id).Count();
                    HttpContext.Session.SetInt32(Utility.SD.SessionCart, cartCount);

                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
        }
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
