using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Data;
using System.Linq;

namespace MinesweeperWebApp.Controllers
{
    public class HallOfFameController : Controller
    {
        // Milestone 4:
        // This gives the controller access to the database.
        private readonly ApplicationDbContext _context;

        // Milestone 4:
        // Constructor used to bring in the database context.
        public HallOfFameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Milestone 4:
        // Shows the top winning scores on the Hall Of Fame page.
        public IActionResult Index()
        {
            var topScores = _context.HallOfFameScores
                .OrderByDescending(score => score.Score)
                .ThenBy(score => score.TimeSeconds)
                .Take(10)
                .ToList();

            return View(topScores);
        }
    }
}