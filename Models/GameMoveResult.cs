using System.Collections.Generic;

namespace MinesweeperWebApp.Models
{
    // Holds the result of a player move so the page can update cleanly.
    public class GameMoveResult
    {
        public string ImageUrl { get; set; }
        public int Score { get; set; }
        public string Time { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }
        public bool IsFlagged { get; set; }

        // Tells the page when the gold bag bonus was found.
        public bool FoundGoldBag { get; set; }

        // Message shown when the gold bag is found.
        public string GoldMessage { get; set; }

        public List<ChangedCell> ChangedCells { get; set; } = new List<ChangedCell>();
        public Board UpdatedBoard { get; set; }
    }
}