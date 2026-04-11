using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // This constructor gives us access to logging if we need it.
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Loads the main home page.
        public IActionResult Index()
        {
            return View();
        }

        // Loads the privacy page.
        public IActionResult Privacy()
        {
            return View();
        }

        // Simple placeholder page for Hall Of Fame.
        // Temporary view path for now.
        public IActionResult HallOfFame()
        {
            return View("~/Views/HallOfFame/Index.cshtml");
        }

        // Loads the shared error page if something goes wrong.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}