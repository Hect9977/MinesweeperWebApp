using System.Collections.Generic;

namespace MinesweeperWebApp.Models
{
    // Milestone 3:
    // This class holds the results from a board click so the controller can send them back to the page
    public class GameMoveResult
    {
        // Milestone 3:
        // Stores the image that should be shown for the clicked cell
        public string ImageUrl { get; set; }

        // Milestone 3:
        // Stores the updated score after the move
        public int Score { get; set; }

        // Milestone 3:
        // Stores the formatted game time to display on the board
        public string Time { get; set; }

        // Milestone 3:
        // Tells the controller if the player hit a mine
        public bool IsGameOver { get; set; }

        // Milestone 3:
        // Tells the controller if the player won the game
        public bool IsWin { get; set; }

        // Milestone 3:
        // Tells the page if the clicked cell is flagged
        public bool IsFlagged { get; set; }

        // Milestone 3:
        // Stores every cell that changed after the move
        public List<ChangedCell> ChangedCells { get; set; } = new List<ChangedCell>();

        // Milestone 3:
        // Stores the updated board after the move
        public Board UpdatedBoard { get; set; }
    }
}