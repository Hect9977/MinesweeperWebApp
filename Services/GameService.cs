using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Services
{
    // Milestone 3:
    // This service handles board click logic so the controller does not have to do all the work
    public class GameService
    {
        // Milestone 3:
        // This method reveals the clicked cell and builds the result we need for the page update
        public GameMoveResult ProcessLeftClick(Board board, int row, int col, string startTimeString)
        {
            Cell cell = board.Cells[row][col];

            // Milestone 3:
            // Do not reveal a flagged cell when the player left-clicks it
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

            // Reveal the selected cell
            board.RevealCell(row, col);

            return new GameMoveResult
            {
                ImageUrl = GetImageUrl(board.Cells[row][col]),
                Score = board.Score,
                Time = GetFormattedElapsedTime(startTimeString),
                IsGameOver = board.IsGameOver,
                IsWin = board.IsWin,
                IsFlagged = board.Cells[row][col].IsFlagged,
                ChangedCells = GetChangedCells(board),
                UpdatedBoard = board
            };
        }

        // Milestone 3:
        // This method adds or removes a flag from the selected cell
        public GameMoveResult ProcessRightClick(Board board, int row, int col, string startTimeString)
        {
            Cell cell = board.Cells[row][col];

            // Milestone 3:
            // Do not allow flags on cells that are already revealed
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

            // Milestone 3:
            // Switch the flag on or off when the player right-clicks
            cell.IsFlagged = !cell.IsFlagged;

            // Milestone 3:
            // Check if the board is now complete after the flag change
            board.UpdateWinStatus();

            return new GameMoveResult
            {
                ImageUrl = cell.IsFlagged ? "/img/flag.png" : "/img/hidden.png",
                Score = board.Score,
                Time = GetFormattedElapsedTime(startTimeString),
                IsGameOver = board.IsGameOver,
                IsWin = board.IsWin,
                IsFlagged = cell.IsFlagged,
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

        // Milestone 3:
        // This helper method collects every cell that should now be visible on the page
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

        // Milestone 3:
        // This helper method returns the correct image for a cell
        private string GetImageUrl(Cell cell)
        {
            if (cell.IsFlagged)
            {
                return "/img/flag.png";
            }
            else if (!cell.IsVisited)
            {
                return "/img/hidden.png";
            }
            else if (cell.HasMine)
            {
                return "/img/mine.png";
            }
            else if (cell.LiveNeighbors > 0)
            {
                return $"/img/tile{cell.LiveNeighbors}.png";
            }
            else
            {
                return "/img/revealed.png";
            }
        }

        // Milestone 3:
        // This helper method formats the elapsed game time
        private string GetFormattedElapsedTime(string startTimeString)
        {
            string elapsedTimeFormatted = "00:00";

            if (!string.IsNullOrEmpty(startTimeString))
            {
                // Milestone 3:
                // Read the saved start time so the service can calculate elapsed time correctly
                DateTime startTime = DateTime.Parse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);

                // Calculate how long the player has been in the current game
                TimeSpan elapsedTime = DateTime.UtcNow - startTime;

                // Format the time so it looks clean on the board
                elapsedTimeFormatted = elapsedTime.ToString(@"mm\:ss");
            }

            return elapsedTimeFormatted;
        }
    }
}