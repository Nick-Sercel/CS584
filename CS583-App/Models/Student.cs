using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS583_App.Models
{
    public class Student
    {
        public required int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public int SubjectId { get; set; }

        [ValidateNever] // Ignore this property during model validation
        public Subject Major { get; set; } = null!;

        public List<Course> Courses { get; set; } = new List<Course>();
    }
}
