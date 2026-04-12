using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Services;
using System.Text.Json;
using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        // This gives us access to the score calculation logic.
        private readonly ScoreService _scoreService;

        // This is used to randomly choose funny messages.
        private readonly Random _random = new Random();

        // Constructor to bring in the ScoreService.
        public GameController(ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        // Default page for Game.
        public IActionResult Index()
        {
            return View();
        }

        // This page is where the user starts a game.
        // We check if they are logged in before letting them continue.
        public IActionResult StartGame()
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");

            // If the user is not logged in, send them back to login.
            if (loggedIn != "true")
            {
                TempData["Message"] = "You must log in before starting a new game.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // From Hector:
        // Update: This version of the method saves the game settings and board to session, then redirects to a new action that loads the board page.
        // This way we can ensure only the player who started the game can access the board.
        [HttpPost]
        public IActionResult MineSweeperBoard(int boardSize, string difficulty)
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            string username = HttpContext.Session.GetString("Username");

            if (loggedIn != "true" || string.IsNullOrEmpty(username))
            {
                TempData["Message"] = "You must log in before viewing the game board.";
                return RedirectToAction("Login", "Account");
            }

            HttpContext.Session.SetString("GameOwner", username);
            HttpContext.Session.SetInt32("BoardSize", boardSize);
            HttpContext.Session.SetString("Difficulty", difficulty);

            Board board = new Board(boardSize);

            string boardJson = JsonSerializer.Serialize(board);
            HttpContext.Session.SetString("CurrentBoard", boardJson);

            return RedirectToAction("LoadMineSweeperBoard");
        }

        // Update: This new action loads the board page, but first checks if the user is logged in and is the owner of the game before allowing access.
        public IActionResult LoadMineSweeperBoard()
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            string username = HttpContext.Session.GetString("Username");
            string gameOwner = HttpContext.Session.GetString("GameOwner");

            if (loggedIn != "true" || string.IsNullOrEmpty(username) || gameOwner != username)
            {
                TempData["Message"] = "Only the player who started this game can access the board.";
                return RedirectToAction("Login", "Account");
            }

            int boardSize = HttpContext.Session.GetInt32("BoardSize") ?? 8;
            string difficulty = HttpContext.Session.GetString("Difficulty") ?? "Easy";

            string boardJson = HttpContext.Session.GetString("CurrentBoard");
            Board board = string.IsNullOrEmpty(boardJson)
                ? new Board(boardSize)
                : JsonSerializer.Deserialize<Board>(boardJson);

            ViewBag.BoardSize = boardSize;
            ViewBag.Difficulty = difficulty;
            ViewBag.GameOwner = username;

            return View("MineSweeperBoard", board);
        }

        // This method is called when the player clicks on a cell to reveal it.
        [HttpPost]
        public IActionResult LeftClick(int row, int col)
        {
            string boardJson = HttpContext.Session.GetString("CurrentBoard");

            if (string.IsNullOrEmpty(boardJson))
            {
                return RedirectToAction("StartGame");
            }

            Board board = JsonSerializer.Deserialize<Board>(boardJson);

            board.RevealCell(row, col);

            HttpContext.Session.SetString("CurrentBoard", JsonSerializer.Serialize(board));

            return RedirectToAction("LoadMineSweeperBoard");
        }

        // This page is shown when the player wins the game.
        public IActionResult Win()
        {
            // These are placeholder values for now.
            // Later we will replace these with real game data.
            int elapsedSeconds = 30;
            int boardSize = 8;
            int difficulty = 2;

            // Calculate the score using the service.
            int score = _scoreService.CalculateScore(elapsedSeconds, boardSize, difficulty);

            // Send the score to the view so it can be displayed.
            ViewBag.Score = score;

            // Send random funny messages to the page.
            ViewBag.WinMessage = GetRandomWinMessage();
            ViewBag.ScoreMessage = GetRandomScoreMessage();

            return View();
        }

        // This page is shown when the player loses the game.
        public IActionResult Loss()
        {
            // Send a random funny loss message to the page.
            ViewBag.LossMessage = GetRandomLossMessage();

            return View();
        }

        // Picks one random win message.
        private string GetRandomWinMessage()
        {
            string[] messages =
            {
                "You cleared the board like you had insider info. Suspicious... but impressive.",
                "Not a single mine touched you. The mines are filing a complaint.",
                "That was clean. Too clean. Are you part robot?",
                "You just made Minesweeper look easy... and we both know it is not.",
                "Every safe click, no panic. That is how legends play.",
                "The mines tried... they really did... just not hard enough."
            };

            return messages[_random.Next(messages.Length)];
        }

        // Picks one random loss message.
        private string GetRandomLossMessage()
        {
            string[] messages =
            {
                "Boom. That mine had your name written all over it.",
                "That click felt right... until it absolutely was not.",
                "The board said no and meant it.",
                "You found a mine. Unfortunately, it found you first.",
                "That was a bold move... just not a safe one.",
                "One wrong click and the whole board took it personally."
            };

            return messages[_random.Next(messages.Length)];
        }

        // Picks one random score message.
        private string GetRandomScoreMessage()
        {
            string[] messages =
            {
                "That score? Yeah... that is worth bragging about a little.",
                "Not bad at all. The mines definitely felt that one.",
                "Solid score. You did not just survive... you showed off.",
                "That number is looking pretty nice up there.",
                "Respect. That score did not come easy.",
                "That is a run it back and beat it kind of score."
            };

            return messages[_random.Next(messages.Length)];
        }
    }
}