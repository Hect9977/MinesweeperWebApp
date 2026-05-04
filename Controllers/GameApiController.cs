using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Data;
using System.Linq;

namespace MinesweeperWebApp.Controllers
{
    // Milestone 4:
    // This controller handles API requests for saved game data.
    [ApiController]
    [Route("api/[controller]")]
    public class GameApiController : ControllerBase
    {
        // This gives the controller access to the database.
        private readonly ApplicationDbContext _context;

        // Constructor used to bring in the database context.
        public GameApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Milestone 4:
        // Returns all saved games from the database as JSON.
        [HttpGet]
        public IActionResult GetAllSavedGames()
        {
            var savedGames = _context.Games.ToList();

            return Ok(savedGames);
        }

        // Milestone 4:
        // Returns one saved game based on its ID.
        [HttpGet("{id}")]
        public IActionResult GetSavedGameById(int id)
        {
            var savedGame = _context.Games.FirstOrDefault(g => g.Id == id);

            // Return a clear message if the saved game is not found.
            if (savedGame == null)
            {
                return NotFound("Saved game was not found.");
            }

            return Ok(savedGame);
        }

        // Milestone 4:
        // Deletes one saved game by ID through the API.
        [HttpDelete("{id}")]
        public IActionResult DeleteSavedGameById(int id)
        {
            var savedGame = _context.Games.FirstOrDefault(g => g.Id == id);

            // Return a clear message if the saved game is not found.
            if (savedGame == null)
            {
                return NotFound("Saved game was not found.");
            }

            _context.Games.Remove(savedGame);
            _context.SaveChanges();

            return Ok("Saved game deleted successfully.");
        }
    }
}