using System;

namespace MinesweeperWebApp.Models
{
    // Milestone 4:
    // This stores one completed winning game for the Hall of Fame page.
    public class HallOfFameScore
    {
        public int Id { get; set; }

        // Milestone 4:
        // This connects the score to the player who won the game.
        public int UserId { get; set; }

        // Milestone 4:
        // This stores the username so the Hall of Fame page can show who earned the score.
        public string Username { get; set; } = string.Empty;

        public int Score { get; set; }

        public int BoardSize { get; set; }

        public string Difficulty { get; set; } = string.Empty;

        public int TimeSeconds { get; set; }

        public DateTime DateWon { get; set; }
    }
}