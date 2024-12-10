using System.ComponentModel.DataAnnotations;

namespace CS583_App.Models
{
    public class Subject
    {
        [Key]
        [Required]
        public required int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }
        public Subject() { }
    }
}
