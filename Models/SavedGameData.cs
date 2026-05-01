namespace MinesweeperWebApp.Models
{
    /// Milestone 4: This class represents the data structure for a saved game, including user information and the game state.
    public class SavedGameData
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int BoardSize { get; set; }

        public string Difficulty { get; set; } = string.Empty;

        public string StartTime { get; set; } = string.Empty;

        public Board Board { get; set; } = new Board();
    }
}