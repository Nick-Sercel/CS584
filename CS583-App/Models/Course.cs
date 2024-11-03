using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS583_App.Models
{
    public class Course
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Navigation properties
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        //public int SubjectId { get; set; }
        //public Subject Subject { get; set; } = null!;

        public List<Student> Students { get; set; } = new List<Student>();
    }
}
