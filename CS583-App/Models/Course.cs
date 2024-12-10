using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS583_App.Models
{
    public class Course
    {
        public required int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Teacher is required.")]
        public int TeacherId { get; set; }
        [ValidateNever] // Ignore this property during model validation
        public Teacher Teacher { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required.")]
        public int SubjectId { get; set; }
        [ValidateNever] // Ignore this property during model validation
        public Subject Subject { get; set; } = null!;

        public List<Student> Students { get; set; } = new List<Student>();
    }
}
