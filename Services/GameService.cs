using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Services
{
    // Handles board click logic so the controller can stay cleaner.
    public class GameService
    {
        private readonly Random _random = new Random();

        // Reveals the clicked cell and builds the result used to update the page.
        public GameMoveResult ProcessLeftClick(Board board, int row, int col, string startTimeString)
        {
            Cell cell = board.Cells[row][col];

            // Do not reveal a flagged cell when the player left-clicks it.
            if (cell.IsFlagged)
            {
                return new GameMoveResult
                {
                    ImageUrl = "/img/flag.png",
                    Score = board.Score,
                    Time = GetFormattedElapsedTime(startTimeString),
                    IsGameOver = board.IsGameOver,
                    IsWin = board.IsWin,
                    IsFlagged = true,
                    FoundGoldBag = false,
                    GoldMessage = "",
                    ChangedCells = new List<ChangedCell>
                    {
                        new ChangedCell
                        {
                            Row = row,
                            Col = col,
                            ImageUrl = "/img/flag.png",
                            IsFlagged = true
                        }
                    },
                    UpdatedBoard = board
                };
            }

            // Reveal the selected cell.
            board.RevealCell(row, col);

            return new GameMoveResult
            {
                ImageUrl = GetImageUrl(board.Cells[row][col]),
                Score = board.Score,
                Time = GetFormattedElapsedTime(startTimeString),
                IsGameOver = board.IsGameOver,
                IsWin = board.IsWin,
                IsFlagged = board.Cells[row][col].IsFlagged,
                FoundGoldBag = board.GoldBagFoundThisMove,
                GoldMessage = board.GoldBagFoundThisMove ? GetRandomGoldMessage() : "",
                ChangedCells = GetChangedCells(board),
                UpdatedBoard = board
            };
        }

        // Adds or removes a flag from the selected cell.
        public GameMoveResult ProcessRightClick(Board board, int row, int col, string startTimeString)
        {
            Cell cell = board.Cells[row][col];

            // Do not allow flags on cells that are already revealed.
            if (cell.IsVisited)
            {
                return new GameMoveResult
                {
                    ImageUrl = GetImageUrl(cell),
                    Score = board.Score,
                    Time = GetFormattedElapsedTime(startTimeString),
                    IsGameOver = board.IsGameOver,
                    IsWin = board.IsWin,
                    IsFlagged = cell.IsFlagged,
                    FoundGoldBag = false,
                    GoldMessage = "",
                    ChangedCells = new List<ChangedCell>
                    {
                        new ChangedCell
                        {
                            Row = row,
                            Col = col,
                            ImageUrl = GetImageUrl(cell),
                            IsFlagged = cell.IsFlagged
                        }
                    },
                    UpdatedBoard = board
                };
            }

            // Switch the flag on or off when the player right-clicks.
            cell.IsFlagged = !cell.IsFlagged;

            // Check if the board is complete after the flag change.
            board.UpdateWinStatus();

            return new GameMoveResult
            {
                ImageUrl = cell.IsFlagged ? "/img/flag.png" : "/img/hidden.png",
                Score = board.Score,
                Time = GetFormattedElapsedTime(startTimeString),
                IsGameOver = board.IsGameOver,
                IsWin = board.IsWin,
                IsFlagged = cell.IsFlagged,
                FoundGoldBag = false,
                GoldMessage = "",
                ChangedCells = new List<ChangedCell>
                {
                    new ChangedCell
                    {
                        Row = row,
                        Col = col,
                        ImageUrl = cell.IsFlagged ? "/img/flag.png" : "/img/hidden.png",
                        IsFlagged = cell.IsFlagged
                    }
                },
                UpdatedBoard = board
            };
        }

        // Collects every cell that should now be visible on the page.
        private List<ChangedCell> GetChangedCells(Board board)
        {
            List<ChangedCell> changedCells = new List<ChangedCell>();

            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    Cell cell = board.Cells[row][col];

                    if (cell.IsVisited || cell.IsFlagged)
                    {
                        changedCells.Add(new ChangedCell
                        {
                            Row = row,
                            Col = col,
                            ImageUrl = GetImageUrl(cell),
                            IsFlagged = cell.IsFlagged
                        });
                    }
                }
            }

            return changedCells;
        }

        // Returns the correct image for a cell.
        private string GetImageUrl(Cell cell)
        {
            if (cell.IsFlagged)
            {
                return "/img/flag.png";
            }

            if (!cell.IsVisited)
            {
                return "/img/hidden.png";
            }

            if (cell.HasMine)
            {
                return "/img/mine.png";
            }

            if (cell.HasGoldBag)
            {
                return "/img/Gold.png";
            }

            if (cell.LiveNeighbors > 0)
            {
                return $"/img/tile{cell.LiveNeighbors}.png";
            }

            return "/img/revealed.png";
        }

        // Picks a funny message when the player finds the gold bag.
        private string GetRandomGoldMessage()
        {
            string[] messages =
            {
                "You found the gold bag. Look at you accidentally making good decisions.",
                "Gold bag found. The board finally decided to be nice for once.",
                "You found bonus gold. Try not to act too humble about it.",
                "That gold bag just saved your score from looking sad.",
                "Well well well... somebody found the shiny little cheat code."
            };

            return messages[_random.Next(messages.Length)];
        }

        // Formats the elapsed game time for the board page.
        private string GetFormattedElapsedTime(string startTimeString)
        {
            string elapsedTimeFormatted = "00:00";

            if (!string.IsNullOrEmpty(startTimeString))
            {
                DateTime startTime = DateTime.Parse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
                TimeSpan elapsedTime = DateTime.UtcNow - startTime;

                elapsedTimeFormatted = elapsedTime.ToString(@"mm\:ss");
            }

            return elapsedTimeFormatted;
        }
    }
}