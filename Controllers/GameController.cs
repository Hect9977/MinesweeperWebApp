using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinesweeperWebApp.Services;
using System.Text.Json;
using MinesweeperWebApp.Models;
using MinesweeperWeb.Data;
using System.Linq;

namespace MinesweeperWebApp.Controllers
{
    public class GameController : Controller
    {
        // Gives this controller access to the score calculation logic.
        private readonly ScoreService _scoreService;

        // Gives this controller access to the game click logic.
        private readonly GameService _gameService;

        // Gives this controller access to the database.
        private readonly ApplicationDbContext _context;

        // Used to randomly pick win, loss, and score messages.
        private readonly Random _random = new Random();

        // Brings in the services this controller needs.
        public GameController(ScoreService scoreService, GameService gameService, ApplicationDbContext context)
        {
            _scoreService = scoreService;
            _gameService = gameService;
            _context = context;
        }

        // Opens the default Game page if it is ever needed.
        public IActionResult Index()
        {
            return View();
        }

        // Opens the Start Game page after checking that the player is logged in.
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

        // Starts a new game by saving the game settings and board into session.
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

            // Create the board using the selected difficulty so Hard mode can include the gold bag.
            Board board = new Board(boardSize, difficulty);

            string boardJson = JsonSerializer.Serialize(board);
            HttpContext.Session.SetString("CurrentBoard", boardJson);

            // Save the start time so the live timer can run on the board page.
            HttpContext.Session.SetString("StartTime", DateTime.UtcNow.ToString("O"));

            // Clear old final results so the next game starts fresh.
            HttpContext.Session.Remove("FinalScore");
            HttpContext.Session.Remove("FinalTimeSeconds");
            HttpContext.Session.Remove("FinalBoardSize");
            HttpContext.Session.Remove("FinalDifficulty");
            HttpContext.Session.Remove("HallOfFameScoreSaved");

            return RedirectToAction("LoadMineSweeperBoard");
        }

        // Loads the current board from session and sends it to the board page.
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
            ViewBag.StartTime = HttpContext.Session.GetString("StartTime");

            return View("MineSweeperBoard", board);
        }

        // Handles a left-click on the board and returns the updated cell data as JSON.
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
            string startTimeString = HttpContext.Session.GetString("StartTime");

            GameMoveResult result = _gameService.ProcessLeftClick(board, row, col, startTimeString);

            // Save the updated board and score after the player makes a move.
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
                foundGoldBag = result.FoundGoldBag,
                goldMessage = result.GoldMessage,
                changedCells = result.ChangedCells,
                lossUrl = Url.Action("Loss", "Game"),
                winUrl = Url.Action("Win", "Game")
            });
        }

        // Handles a right-click on the board so the player can place or remove a flag.
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
            string startTimeString = HttpContext.Session.GetString("StartTime");

            GameMoveResult result = _gameService.ProcessRightClick(board, row, col, startTimeString);

            // Save the board after the flag state changes.
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

        // Saves the current game to the database as JSON.
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

            // Build one object that keeps all saved game details together.
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

        // Shows all saved games for the logged-in player.
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

        // Loads a selected saved game and puts that game back into session.
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

        // Deletes a selected saved game from the database.
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

        // Shows the Winner page and keeps the final score details locked in.
        public IActionResult Win()
        {
            int? savedFinalScore = HttpContext.Session.GetInt32("FinalScore");
            int? savedFinalTime = HttpContext.Session.GetInt32("FinalTimeSeconds");
            int? savedFinalBoardSize = HttpContext.Session.GetInt32("FinalBoardSize");
            string savedFinalDifficulty = HttpContext.Session.GetString("FinalDifficulty");

            int score;
            int elapsedSeconds;
            int boardSize;
            string difficultyString;

            // Use the same final results if the Winner page reloads.
            if (savedFinalScore != null &&
                savedFinalTime != null &&
                savedFinalBoardSize != null &&
                !string.IsNullOrEmpty(savedFinalDifficulty))
            {
                score = savedFinalScore.Value;
                elapsedSeconds = savedFinalTime.Value;
                boardSize = savedFinalBoardSize.Value;
                difficultyString = savedFinalDifficulty;
            }
            else
            {
                boardSize = HttpContext.Session.GetInt32("BoardSize") ?? 8;
                difficultyString = HttpContext.Session.GetString("Difficulty") ?? "Easy";

                int difficulty = GetDifficultyValue(difficultyString);
                elapsedSeconds = GetElapsedSeconds();

                score = _scoreService.CalculateScore(elapsedSeconds, boardSize, difficulty);

                // Save the final results one time so the Winner page and Hall Of Fame match.
                HttpContext.Session.SetInt32("FinalScore", score);
                HttpContext.Session.SetInt32("FinalTimeSeconds", elapsedSeconds);
                HttpContext.Session.SetInt32("FinalBoardSize", boardSize);
                HttpContext.Session.SetString("FinalDifficulty", difficultyString);
            }

            ViewBag.Score = score;
            ViewBag.Time = elapsedSeconds;
            ViewBag.BoardSize = boardSize;
            ViewBag.Difficulty = difficultyString;
            ViewBag.GameOwner = HttpContext.Session.GetString("Username");

            ViewBag.WinMessage = GetRandomWinMessage();
            ViewBag.ScoreMessage = GetRandomScoreMessage();

            return View();
        }

        // Saves the winning score to the Hall Of Fame table using the player's chosen display name.
        [HttpPost]
        public IActionResult SaveHallOfFameScore(string displayName)
        {
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");
            string scoreSaved = HttpContext.Session.GetString("HallOfFameScoreSaved");

            if (loggedIn != "true" || userId == null)
            {
                TempData["Message"] = "You must be logged in to save a Hall Of Fame score.";
                return RedirectToAction("Login", "Account");
            }

            if (scoreSaved == "true")
            {
                TempData["HallOfFameMessage"] = "This score has already been saved.";
                return RedirectToAction("Win");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                TempData["HallOfFameMessage"] = "Please enter a name before saving your score.";
                return RedirectToAction("Win");
            }

            int score = HttpContext.Session.GetInt32("FinalScore") ?? 0;
            int timeSeconds = HttpContext.Session.GetInt32("FinalTimeSeconds") ?? 0;
            int boardSize = HttpContext.Session.GetInt32("FinalBoardSize") ?? 8;
            string difficulty = HttpContext.Session.GetString("FinalDifficulty") ?? "Easy";

            HallOfFameScore hallOfFameScore = new HallOfFameScore
            {
                UserId = userId.Value,
                Username = displayName.Trim(),
                Score = score,
                BoardSize = boardSize,
                Difficulty = difficulty,
                TimeSeconds = timeSeconds,
                DateWon = DateTime.Now
            };

            _context.HallOfFameScores.Add(hallOfFameScore);
            _context.SaveChanges();

            HttpContext.Session.SetString("HallOfFameScoreSaved", "true");
            TempData["HallOfFameMessage"] = "Score saved to Hall Of Fame!";

            return RedirectToAction("Win");
        }

        // Shows the Game Over page with the final game details.
        public IActionResult Loss()
        {
            int boardSize = HttpContext.Session.GetInt32("BoardSize") ?? 8;
            string difficultyString = HttpContext.Session.GetString("Difficulty") ?? "Easy";
            int difficulty = GetDifficultyValue(difficultyString);
            int elapsedSeconds = GetElapsedSeconds();

            int score = HttpContext.Session.GetInt32("CurrentScore")
                ?? _scoreService.CalculateScore(elapsedSeconds, boardSize, difficulty);

            ViewBag.Score = score;
            ViewBag.Time = elapsedSeconds;
            ViewBag.BoardSize = boardSize;
            ViewBag.Difficulty = difficultyString;
            ViewBag.LossMessage = GetRandomLossMessage();

            return View();
        }

        // Converts the difficulty word into the number used for scoring.
        private int GetDifficultyValue(string difficultyString)
        {
            if (difficultyString == "Medium")
            {
                return 2;
            }

            if (difficultyString == "Hard")
            {
                return 3;
            }

            return 1;
        }

        // Calculates how many seconds the player spent in the current game.
        private int GetElapsedSeconds()
        {
            string startTimeString = HttpContext.Session.GetString("StartTime");

            if (string.IsNullOrEmpty(startTimeString))
            {
                return 0;
            }

            DateTime startTime = DateTime.Parse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
            TimeSpan elapsedTime = DateTime.UtcNow - startTime;

            return (int)elapsedTime.TotalSeconds;
        }

        // Picks one random win message for the Winner page.
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

        // Picks one random loss message for the Game Over page.
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

        // Picks one random score message for the Winner page.
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

        // Collects visible cells after the gold bag test button is used.
        private List<ChangedCell> GetVisibleCellsForTest(Board board)
        {
            List<ChangedCell> changedCells = new List<ChangedCell>();

            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    Cell cell = board.Cells[row][col];

                    if (cell.IsVisited || cell.IsFlagged)
                    {
                        string imageUrl = "/img/revealed.png";

                        if (cell.IsFlagged)
                        {
                            imageUrl = "/img/flag.png";
                        }
                        else if (cell.HasMine)
                        {
                            imageUrl = "/img/mine.png";
                        }
                        else if (cell.HasGoldBag)
                        {
                            imageUrl = "/img/Gold.png";
                        }
                        else if (cell.LiveNeighbors > 0)
                        {
                            imageUrl = $"/img/tile{cell.LiveNeighbors}.png";
                        }

                        changedCells.Add(new ChangedCell
                        {
                            Row = row,
                            Col = col,
                            ImageUrl = imageUrl,
                            IsFlagged = cell.IsFlagged
                        });
                    }
                }
            }

            return changedCells;
        }

        // Picks one random message when the gold bag is found.
        private string GetRandomGoldMessage()
        {
            string[] messages =
            {
                "You found the gold bag. Look at you accidentally making good decisions.",
                "Gold bag found. The mines are now financially embarrassed.",
                "You found bonus gold. Try not to act brand new.",
                "Well look at that. A good click finally showed up."
            };

            return messages[_random.Next(messages.Length)];
        }
    }
}