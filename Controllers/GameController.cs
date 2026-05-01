using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Services;
using System.Text.Json;
using MinesweeperWebApp.Models;
using MinesweeperWeb.Data;
using MinesweeperWeb.Models;
using System.Linq;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        // This gives us access to the score calculation logic.
        private readonly ScoreService _scoreService;

        // Milestone 3:
        // This gives us access to the game service so click logic can stay out of the controller
        private readonly GameService _gameService;

        // Milestone 4 Part 1:
        // This gives us access to the database so we can save games.
        private readonly ApplicationDbContext _context;

        // This is used to randomly choose funny messages.
        private readonly Random _random = new Random();

        // Constructor to bring in the ScoreService.
        public GameController(ScoreService scoreService, GameService gameService, ApplicationDbContext context)
        {
            _scoreService = scoreService;

            // Milestone 3:
            // Save the game service so we can use it in the click action
            _gameService = gameService;

            // Milestone 4:
            // Save the database context so the current game can be saved.
            _context = context;
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

            // Milestone 3:
            // Save the game start time in a clean format so the timer can use it on the page
            HttpContext.Session.SetString("StartTime", DateTime.UtcNow.ToString("O"));

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
            ViewBag.Score = board.Score;
            ViewBag.ShowLossDelay = TempData["ShowLossDelay"] as string;

            // Milestone 3:
            // Send the saved start time to the board view so the timer can run live on the page
            ViewBag.StartTime = HttpContext.Session.GetString("StartTime");

            return View("MineSweeperBoard", board);
        }

        // Milestone 3:
        // This method is called when the player clicks on a cell to reveal it.
        // It now returns JSON so the board can update without reloading the whole page.
        [HttpPost]
        public IActionResult LeftClick(int row, int col)
        {
            string boardJson = HttpContext.Session.GetString("CurrentBoard");

            if (string.IsNullOrEmpty(boardJson))
            {
                return Json(new
                {
                    success = false,
                    redirectUrl = Url.Action("StartGame", "Game")
                });
            }

            Board board = JsonSerializer.Deserialize<Board>(boardJson);

            // Milestone 3:
            // Get the start time from session so the service can calculate elapsed time
            string startTimeString = HttpContext.Session.GetString("StartTime");

            // Milestone 3:
            // Send the board click to the game service so it can process the move
            GameMoveResult result = _gameService.ProcessLeftClick(board, row, col, startTimeString);

            // Milestone 3:
            // Save the updated board and score back to session after the move
            HttpContext.Session.SetString("CurrentBoard", JsonSerializer.Serialize(result.UpdatedBoard));
            HttpContext.Session.SetInt32("CurrentScore", result.Score);

            return Json(new
            {
                success = true,
                row = row,
                col = col,
                imageUrl = result.ImageUrl,
                score = result.Score,
                time = result.Time,
                isGameOver = result.IsGameOver,
                isWin = result.IsWin,
                changedCells = result.ChangedCells,
                lossUrl = Url.Action("Loss", "Game"),
                winUrl = Url.Action("Win", "Game")
            });
        }

        // Milestone 3:
        // This method handles right-clicking a cell to add or remove a flag
        [HttpPost]
        public IActionResult RightClick(int row, int col)
        {
            string boardJson = HttpContext.Session.GetString("CurrentBoard");

            if (string.IsNullOrEmpty(boardJson))
            {
                return Json(new
                {
                    success = false,
                    redirectUrl = Url.Action("StartGame", "Game")
                });
            }

            Board board = JsonSerializer.Deserialize<Board>(boardJson);

            // Milestone 3:
            // Get the start time from session so the service can keep the timer updated
            string startTimeString = HttpContext.Session.GetString("StartTime");

            // Milestone 3:
            // Send the right-click action to the game service
            GameMoveResult result = _gameService.ProcessRightClick(board, row, col, startTimeString);

            // Milestone 3:
            // Save the updated board after the flag is changed
            HttpContext.Session.SetString("CurrentBoard", JsonSerializer.Serialize(result.UpdatedBoard));

            return Json(new
            {
                success = true,
                row = row,
                col = col,
                imageUrl = result.ImageUrl,
                score = result.Score,
                time = result.Time,
                isFlagged = result.IsFlagged,
                isWin = result.IsWin
            });
        }

        // Milestone 4:
        // Saves the current Minesweeper game to the database as JSON.
        [HttpPost]
        public IActionResult SaveGame()
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            string username = HttpContext.Session.GetString("Username");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (loggedIn != "true" || string.IsNullOrEmpty(username) || userId == null)
            {
                TempData["Message"] = "You must be logged in before saving a game.";
                return RedirectToAction("Login", "Account");
            }

            string boardJson = HttpContext.Session.GetString("CurrentBoard");

            if (string.IsNullOrEmpty(boardJson))
            {
                TempData["SaveMessage"] = "No active game was found to save.";
                return RedirectToAction("StartGame", "Game");
            }

            int boardSize = HttpContext.Session.GetInt32("BoardSize") ?? 8;
            string difficulty = HttpContext.Session.GetString("Difficulty") ?? "Easy";
            string startTime = HttpContext.Session.GetString("StartTime") ?? DateTime.UtcNow.ToString("O");

            // This object contains the board and extra game information.
            var gameDataObject = new
            {
                UserId = userId.Value,
                Username = username,
                BoardSize = boardSize,
                Difficulty = difficulty,
                StartTime = startTime,
                Board = JsonSerializer.Deserialize<Board>(boardJson)
            };

            string gameDataJson = JsonSerializer.Serialize(gameDataObject);

            SavedGame savedGame = new SavedGame
            {
                UserId = userId.Value,
                DateSaved = DateTime.Now,
                Gamedata = gameDataJson
            };

            _context.Games.Add(savedGame);
            _context.SaveChanges();

            TempData["SaveMessage"] = "Game saved successfully!";

            return RedirectToAction("LoadMineSweeperBoard");
        }

        // Milestone 4:
        // Shows all saved games for the currently logged-in user.
        public IActionResult ShowSavedGames()
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (loggedIn != "true" || userId == null)
            {
                TempData["Message"] = "You must be logged in to view saved games.";
                return RedirectToAction("Login", "Account");
            }

            var savedGames = _context.Games
                .Where(g => g.UserId == userId.Value)
                .OrderByDescending(g => g.DateSaved)
                .ToList();

            return View(savedGames);
        }

        // Milestone 4:
        // Loads one saved game and puts it back into session.
        [HttpPost]
        public IActionResult LoadSavedGame(int id)
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            string username = HttpContext.Session.GetString("Username");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (loggedIn != "true" || string.IsNullOrEmpty(username) || userId == null)
            {
                TempData["Message"] = "You must be logged in to load a saved game.";
                return RedirectToAction("Login", "Account");
            }

            SavedGame savedGame = _context.Games
                .FirstOrDefault(g => g.Id == id && g.UserId == userId.Value);

            if (savedGame == null)
            {
                TempData["SaveMessage"] = "Saved game was not found.";
                return RedirectToAction("ShowSavedGames");
            }

            SavedGameData savedGameData = JsonSerializer.Deserialize<SavedGameData>(savedGame.Gamedata);

            if (savedGameData == null || savedGameData.Board == null)
            {
                TempData["SaveMessage"] = "Saved game data could not be loaded.";
                return RedirectToAction("ShowSavedGames");
            }

            HttpContext.Session.SetString("GameOwner", username);
            HttpContext.Session.SetInt32("BoardSize", savedGameData.BoardSize);
            HttpContext.Session.SetString("Difficulty", savedGameData.Difficulty);
            HttpContext.Session.SetString("StartTime", savedGameData.StartTime);
            HttpContext.Session.SetString("CurrentBoard", JsonSerializer.Serialize(savedGameData.Board));

            TempData["SaveMessage"] = "Saved game loaded successfully!";

            return RedirectToAction("LoadMineSweeperBoard");
        }

        // Milestone 4:
        // Deletes one saved game from the database.
        [HttpPost]
        public IActionResult DeleteSavedGame(int id)
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (loggedIn != "true" || userId == null)
            {
                TempData["Message"] = "You must be logged in to delete a saved game.";
                return RedirectToAction("Login", "Account");
            }

            SavedGame savedGame = _context.Games
                .FirstOrDefault(g => g.Id == id && g.UserId == userId.Value);

            if (savedGame == null)
            {
                TempData["SaveMessage"] = "Saved game was not found.";
                return RedirectToAction("ShowSavedGames");
            }

            _context.Games.Remove(savedGame);
            _context.SaveChanges();

            TempData["SaveMessage"] = "Saved game deleted successfully!";

            return RedirectToAction("ShowSavedGames");
        }

        // This page is shown when the player wins the game.
        public IActionResult Win()
        {
            // Milestone 3:
            // Get the board size from session
            int boardSize = HttpContext.Session.GetInt32("BoardSize") ?? 8;

            // Milestone 3:
            // Get the difficulty from session and convert it to a number
            string difficultyString = HttpContext.Session.GetString("Difficulty") ?? "Easy";

            int difficulty = 1;

            if (difficultyString == "Easy")
            {
                difficulty = 1;
            }
            else if (difficultyString == "Medium")
            {
                difficulty = 2;
            }
            else if (difficultyString == "Hard")
            {
                difficulty = 3;
            }

            // Milestone 3:
            // Get the start time from session
            string startTimeString = HttpContext.Session.GetString("StartTime");

            int elapsedSeconds = 0;

            if (!string.IsNullOrEmpty(startTimeString))
            {
                DateTime startTime = DateTime.Parse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);

                TimeSpan elapsedTime = DateTime.UtcNow - startTime;

                elapsedSeconds = (int)elapsedTime.TotalSeconds;
            }

            // Milestone 3:
            // Calculate the final score using real game data
            int score = _scoreService.CalculateScore(elapsedSeconds, boardSize, difficulty);

            // Send the score to the view so it can be displayed.
            ViewBag.Score = score;

            // Milestone 3:
            // Send extra info to the view so we can display how the score was calculated
            ViewBag.Time = elapsedSeconds;
            ViewBag.BoardSize = boardSize;
            ViewBag.Difficulty = difficultyString;

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