using Microsoft.AspNetCore.Mvc;

namespace AspNetMvcProject.Controllers;

public class ApiTestController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
