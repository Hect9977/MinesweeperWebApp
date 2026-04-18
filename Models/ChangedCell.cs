namespace MinesweeperWebApp.Models
{
    // Milestone 3:
    // This stores the row, column, and image for any cell that changed after a move
    public class ChangedCell
    {
        // Milestone 3:
        // The row of the changed cell
        public int Row { get; set; }

        // Milestone 3:
        // The column of the changed cell
        public int Col { get; set; }

        // Milestone 3:
        // The image that should be shown for this cell
        public string ImageUrl { get; set; }

        // Milestone 3:
        // Tracks whether this cell is flagged on the page
        public bool IsFlagged { get; set; }
    }
}