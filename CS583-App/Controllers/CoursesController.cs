using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CS583_App.Data;
using CS583_App.Models;
using NuGet.DependencyResolver;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

namespace CS583_App.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        [HttpGet]
        [Route("Courses/Index")]
        public IActionResult Index()
        {
            var applicationDbContext = _context.Course.Include(c => c.Subject).Include(c => c.Teacher);
            return Json(applicationDbContext);
        }

        // GET: Courses/Details/5
        [HttpGet]
        [Route("Courses/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return Json(course);
        }

        // POST: Courses/Create
        [HttpPost]
        [Route("Courses/Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var course = new Course
            {
                Id = 0,
                Name = data.GetProperty("Name").GetString(),
                Description = data.GetProperty("Description").GetString(),
                TeacherId = data.GetProperty("TeacherId").GetInt32(),
                SubjectId = data.GetProperty("SubjectId").GetInt32()
            };

            var SelectedStudentIds = data.GetProperty("SelectedStudentIds").EnumerateArray()
                                        .Select(c => c.GetInt32())
                                        .ToList();

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                // Fetch the students from the database based on the selected student IDs
                var selectedStudents = await _context.Student
                    .Where(s => SelectedStudentIds.Contains(s.Id))
                    .ToListAsync();

                // Assign selected students to the course
                course.Students = selectedStudents;
                _context.Add(course);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", course.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name", course.TeacherId);
            ViewBag.Students = await _context.Student.ToListAsync();
            return Json(new { success = false, message = "Failed to create course" });
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [Route("Courses/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var course = new Course
            {
                Id = data.GetProperty("Id").GetInt32(),
                Name = data.GetProperty("Name").GetString(),
                Description = data.GetProperty("Description").GetString(),
                TeacherId = data.GetProperty("TeacherId").GetInt32(),
                SubjectId = data.GetProperty("SubjectId").GetInt32()
            };

            var SelectedStudentIds = data.GetProperty("SelectedStudentIds").EnumerateArray()
                                        .Select(c => c.GetInt32())
                                        .ToList();

            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    var existingCourse = await _context.Course
                        .Include(c => c.Students) // Ensure Students are loaded
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingCourse != null)
                    {

                        // Fetch the students from the database based on the selected student IDs
                        var selectedStudents = await _context.Student
                            .Where(s => SelectedStudentIds.Contains(s.Id))
                            .ToListAsync();

                        existingCourse.Students = selectedStudents;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true });
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", course.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name", course.TeacherId);
            return Json(new { success = false, message = "Failed to edit course" });
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Courses/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course != null)
            {
                _context.Course.Remove(course);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Course deleted successfully." });
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
