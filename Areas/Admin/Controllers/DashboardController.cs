using Microsoft.AspNetCore.Mvc;
using AspNetMvcProject.Utility;

namespace AspNetMvcProject.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // Simple check for Admin role in session
        var role = HttpContext.Session.GetString("Role");
        if (role != SD.Role_Admin)
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        
        return View();
    }
}
