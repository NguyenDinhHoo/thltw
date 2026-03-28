using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Models.ViewModels;
using AspNetMvcProject.Utility;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrderController(ApplicationDbContext db)
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
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<OrderHeader> orderHeaderList = _db.OrderHeaders
            .Where(u => u.ApplicationUserId == user.Id)
            .OrderByDescending(u => u.OrderDate)
            .ToList();

        return View(orderHeaderList);
    }

    public IActionResult Details(int orderId)
    {
        var username = HttpContext.Session.GetString("Username");
        var user = _db.Users.FirstOrDefault(u => u.Username == username);

        OrderHeader orderHeader = _db.OrderHeaders
            .Include(u => u.ApplicationUser)
            .FirstOrDefault(u => u.Id == orderId);

        // Security check
        if (orderHeader == null || orderHeader.ApplicationUserId != user.Id)
        {
            return NotFound();
        }

        var orderDetails = _db.OrderDetails
            .Include(u => u.Product)
            .Where(u => u.OrderHeaderId == orderId)
            .ToList();

        var orderVM = new OrderVM()
        {
            OrderHeader = orderHeader,
            OrderDetails = orderDetails
        };

        return View(orderVM);
    }

    [HttpPost]
    public IActionResult CancelOrder(int orderId)
    {
        var username = HttpContext.Session.GetString("Username");
        var user = _db.Users.FirstOrDefault(u => u.Username == username);

        var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId);

        // Security check
        if (orderHeaderFromDb == null || orderHeaderFromDb.ApplicationUserId != user.Id)
        {
            return NotFound();
        }

        // Only allow cancellation if pending or approved
        if (orderHeaderFromDb.OrderStatus == SD.StatusPending || orderHeaderFromDb.OrderStatus == SD.StatusApproved)
        {
            orderHeaderFromDb.OrderStatus = SD.StatusCancelled;
            orderHeaderFromDb.PaymentStatus = SD.StatusCancelled;
            _db.SaveChanges();
            TempData["success"] = "Đơn hàng đã được hủy thành công.";
        }
        else
        {
            TempData["error"] = "Không thể hủy đơn hàng này vì nó đang được xử lý hoặc đã giao.";
        }

        return RedirectToAction(nameof(Index));
    }
}
