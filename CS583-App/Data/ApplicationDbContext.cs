using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CS583_App.Models;
using Microsoft.AspNetCore.Identity;

namespace CS583_App.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Course> Course { get; set; } = default!;
        public DbSet<Student> Student { get; set; } = default!;
        public DbSet<Teacher> Teacher { get; set; } = default!;
        public DbSet<Subject> Subject { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student - Subject (Major)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Major)
                .WithMany() // Subject has no navigation property to Students
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete on Subject

            // Teacher - Subject (Specialization or Department)
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.Field)
                .WithMany() // Subject has no navigation property to Teachers
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete on Subject

            // Course - Subject (Course Subject)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Subject)
                .WithMany() // Subject has no navigation property to Courses
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete on Subject

            // Configure many-to-many relationship between Student and Course
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Courses)
                .WithMany(c => c.Students);

            // Configure one-to-many relationship between Teacher and Course
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete on Course
        }
    }
}
