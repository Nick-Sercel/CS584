﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS583_App.Models
{
    public class Student
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }

        //public int SubjectId { get; set; }
        //public Subject Major { get; set; } = null!;

        public List<Course> Courses { get; set; } = new List<Course>();
    }
}
