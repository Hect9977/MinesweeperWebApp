using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StartGame()
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");

            if (loggedIn != "true")
            {
                TempData["Message"] = "You must log in before starting a new game.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}