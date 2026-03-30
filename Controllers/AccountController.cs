using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Data;
using MinesweeperWeb.Models;
using System.Linq;

namespace MinesweeperWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("RegisterSuccess");
            }

            return RedirectToAction("RegisterError");
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("LoginError");
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("LoggedIn", "true");
                return RedirectToAction("LoginSuccess");
            }

            return RedirectToAction("LoginError");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult LoginSuccess()
        {
            return View();
        }

        public IActionResult LoginError()
        {
            return View();
        }

        public IActionResult RegisterSuccess()
        {
            return View();
        }

        public IActionResult RegisterError()
        {
            return View();
        }
    }
}