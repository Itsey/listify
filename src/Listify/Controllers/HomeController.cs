using System.Diagnostics;
using Listify.Models;
using Microsoft.AspNetCore.Mvc;
using Plisky.Diagnostics;

namespace Listify.Controllers;

public class HomeController : Controller {
    protected Bilge b = new("HomeController");

    public HomeController() {
    }

    public IActionResult Index() {
        b.Info.Flow();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}