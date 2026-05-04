using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Data;
using MinesweeperWebApp.Models;
using System.Linq;

namespace MinesweeperWebApp.Controllers
{
    // Milestone 4 Part 3:
    // This controller publishes saved games as REST API endpoints.
    [ApiController]
    public class GameApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GameApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/showSavedGames
        // Displays all saved games.
        [HttpGet]
        [Route("api/showSavedGames")]
        public IActionResult ShowSavedGames()
        {
            var savedGames = _context.Games
                .OrderByDescending(g => g.DateSaved)
                .ToList();

            return Ok(savedGames);
        }

        // GET: /api/showSavedGames/5
        // Displays one saved game by ID.
        [HttpGet]
        [Route("api/showSavedGames/{id}")]
        public IActionResult ShowOneSavedGame(int id)
        {
            SavedGame savedGame = _context.Games
                .FirstOrDefault(g => g.Id == id);

            if (savedGame == null)
            {
                return NotFound(new
                {
                    Message = "Saved game was not found.",
                    GameId = id
                });
            }

            return Ok(savedGame);
        }

        // DELETE: /api/deleteOneGame/5
        // Deletes one saved game by ID.
        [HttpDelete]
        [Route("api/deleteOneGame/{id}")]
        public IActionResult DeleteOneGame(int id)
        {
            SavedGame savedGame = _context.Games
                .FirstOrDefault(g => g.Id == id);

            if (savedGame == null)
            {
                return NotFound(new
                {
                    Message = "Saved game was not found.",
                    GameId = id
                });
            }

            _context.Games.Remove(savedGame);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Saved game deleted successfully.",
                GameId = id
            });
        }

        // Optional browser-friendly delete route:
        // GET: /api/deleteOneGame/5
        // This is useful because the milestone lists the delete endpoint like a URL.
        [HttpGet]
        [Route("api/deleteOneGame/{id}")]
        public IActionResult DeleteOneGameFromBrowser(int id)
        {
            SavedGame savedGame = _context.Games
                .FirstOrDefault(g => g.Id == id);

            if (savedGame == null)
            {
                return NotFound(new
                {
                    Message = "Saved game was not found.",
                    GameId = id
                });
            }

            _context.Games.Remove(savedGame);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Saved game deleted successfully.",
                GameId = id
            });
        }
    }
}