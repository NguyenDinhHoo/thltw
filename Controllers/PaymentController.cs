using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Data;
using AspNetMvcProject.Utility;
using AspNetMvcProject.Models;

namespace AspNetMvcProject.Controllers;

public class PaymentController : Controller
{
    private readonly ApplicationDbContext _db;

    public PaymentController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult ProcessPayment(int orderId)
    {
        var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId);
        if (orderHeader == null) return NotFound();

        return View(orderHeader);
    }

    [HttpPost]
    public IActionResult ConfirmPayment(int orderId, bool success)
    {
        var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId);
        if (orderHeader == null) return NotFound();

        if (success)
        {
            orderHeader.PaymentStatus = SD.PaymentStatusApproved;
            orderHeader.OrderStatus = SD.StatusApproved;
            orderHeader.PaymentDate = System.DateTime.Now;
            _db.SaveChanges();
            TempData["success"] = "Thanh toán thành công!";
            return RedirectToAction("OrderConfirmation", "Cart", new { id = orderId });
        }
        else
        {
            orderHeader.PaymentStatus = SD.PaymentStatusRejected;
            _db.SaveChanges();
            TempData["error"] = "Thanh toán bị từ chối hoặc đã hủy.";
            return RedirectToAction("OrderConfirmation", "Cart", new { id = orderId });
        }
    }
}
