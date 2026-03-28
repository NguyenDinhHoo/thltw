using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Utility;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Areas.Admin.Controllers;

[Area("Admin")]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        // Simple role check for Admin
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin)
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var userList = _db.Users.ToList();
        return View(userList);
    }

    [HttpPost]
    public IActionResult ChangeRole(int userId)
    {
        // Simple role check for Admin
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin)
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var userFromDb = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (userFromDb == null)
        {
            return NotFound();
        }

        // Prevent self-role change
        var currentUsername = HttpContext.Session.GetString("Username");
        if (userFromDb.Username == currentUsername)
        {
            TempData["error"] = "Bạn không thể tự thay đổi quyền của chính mình!";
            return RedirectToAction(nameof(Index));
        }

        // Toggle Role
        if (userFromDb.Role == SD.Role_Admin)
        {
            userFromDb.Role = SD.Role_Customer;
        }
        else
        {
            userFromDb.Role = SD.Role_Admin;
        }

        _db.SaveChanges();
        TempData["success"] = "Cập nhật quyền thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int userId)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin) return RedirectToAction("Index", "Home", new { area = "" });

        var userFromDb = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (userFromDb == null) return NotFound();

        return View(userFromDb);
    }

    [HttpPost]
    public IActionResult Edit(ApplicationUser user)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin) return RedirectToAction("Index", "Home", new { area = "" });

        var userFromDb = _db.Users.FirstOrDefault(u => u.Id == user.Id);
        if (userFromDb == null) return NotFound();

        userFromDb.FullName = user.FullName;
        userFromDb.Address = user.Address;
        userFromDb.Age = user.Age;
        userFromDb.Email = user.Email;

        _db.SaveChanges();
        TempData["success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(int userId)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin) return RedirectToAction("Index", "Home", new { area = "" });

        var userFromDb = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (userFromDb == null) return NotFound();

        var currentUsername = HttpContext.Session.GetString("Username");
        if (userFromDb.Username == currentUsername)
        {
            TempData["error"] = "Bạn không thể tự xóa tài khoản của chính mình!";
            return RedirectToAction(nameof(Index));
        }

        _db.Users.Remove(userFromDb);
        _db.SaveChanges();
        TempData["success"] = "Đã xóa người dùng thành công!";
        return RedirectToAction(nameof(Index));
    }
}
