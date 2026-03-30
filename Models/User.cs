using System.ComponentModel.DataAnnotations;

namespace MinesweeperWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Sex { get; set; }

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}