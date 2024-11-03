using System.ComponentModel.DataAnnotations;

namespace CS583_App.Models
{
    public class Subject
    {
        [Key]
        [Required]
        public required int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public Subject() { }
    }
}
