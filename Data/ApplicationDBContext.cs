using Microsoft.EntityFrameworkCore;
using MinesweeperWeb.Models;
using MinesweeperWebApp.Models;

namespace MinesweeperWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        // Milestone 4 Part 1:
        // This table stores saved Minesweeper games.
        public DbSet<SavedGame> Games { get; set; }
    }
}