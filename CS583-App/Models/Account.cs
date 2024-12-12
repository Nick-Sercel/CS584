using System.ComponentModel.DataAnnotations;

namespace CS583_App.Models
{
    public class Account
    {
        [Key]
        [Required]
        public required int Id { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }

    }
}
