namespace MinesweeperWebApp.Models
{
    // Represents one square on the Minesweeper board.
    public class Cell
    {
        public bool HasMine { get; set; }
        public bool IsVisited { get; set; }
        public bool IsFlagged { get; set; }
        public int LiveNeighbors { get; set; }

        // Marks the hidden gold bag bonus tile.
        public bool HasGoldBag { get; set; }
    }
}