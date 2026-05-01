using System.ComponentModel.DataAnnotations;

namespace MinesweeperWebApp.Models
{
    // Milestone: Represents a saved game state for a user.
    public class SavedGame
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime DateSaved { get; set; }

        [Required]
        public string Gamedata { get; set; } = string.Empty;
    }
}
