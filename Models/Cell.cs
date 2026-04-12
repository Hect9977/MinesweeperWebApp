namespace MinesweeperWebApp.Models
{
    // Cell Model representing each cell on the board
    public class Cell
    {
        public bool HasMine { get; set; }
        public bool IsVisited { get; set; }
        public bool IsFlagged { get; set; }
        public int LiveNeighbors { get; set; }
    }
}
