using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using csharp_math.Models;

namespace csharp_math.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Create", "Functions");
    public IActionResult Theory() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
