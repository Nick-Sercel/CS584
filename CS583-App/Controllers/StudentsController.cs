using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CS583_App.Data;
using CS583_App.Models;
using Microsoft.AspNetCore.Authorization;
using NuGet.DependencyResolver;
using System.Text.Json;

namespace CS583_App.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Students
        [HttpGet]
        [Route("Students/Index")]
        public IActionResult Index()
        {
            var applicationDbContext = _context.Student.Include(s => s.Major);
            return Json(applicationDbContext);
        }

        // GET: Students/Details/5
        [HttpGet]
        [Route("Students/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .Include(s => s.Major)
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return Json(student);
        }

        // POST: Students/Create
        [HttpPost]
        [Route("Students/Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var student = new Student
            {
                Id = 0,
                Name = data.GetProperty("Name").GetString(),
                Email = data.GetProperty("Email").GetString(),
                SubjectId = data.GetProperty("SubjectId").GetInt32()
            };

            var SelectedCourseIds = data.GetProperty("SelectedCourseIds").EnumerateArray()
                                        .Select(c => c.GetInt32())
                                        .ToList();

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                // Fetch the courses from the database based on the selected course IDs
                var selectedCourses = await _context.Course
                    .Where(s => SelectedCourseIds.Contains(s.Id))
                    .ToListAsync();

                // Assign selected courses to the student
                student.Courses = selectedCourses;
                _context.Add(student);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", student.SubjectId);
            ViewBag.Courses = await _context.Course.ToListAsync();
            return Json(new { success = false, message = "Failed to create student" });
        }

        // POST: Students/Edit/5
        [HttpPost]
        [Route("Students/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var student = new Student
            {
                Id = data.GetProperty("Id").GetInt32(),
                Name = data.GetProperty("Name").GetString(),
                Email = data.GetProperty("Email").GetString(),
                SubjectId = data.GetProperty("SubjectId").GetInt32()
            };

            var SelectedCourseIds = data.GetProperty("SelectedCourseIds").EnumerateArray()
                                        .Select(c => c.GetInt32())
                                        .ToList();

            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    var existingStudent = await _context.Student
                        .Include(c => c.Courses) // Ensure Courses are loaded
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingStudent != null)
                    {

                        // Fetch the students from the database based on the selected student IDs
                        var selectedCourses = await _context.Course
                            .Where(s => SelectedCourseIds.Contains(s.Id))
                            .ToListAsync();

                        existingStudent.Courses = selectedCourses;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", student.SubjectId);
            return Json(new { success = false, message = "Failed to edit student" });
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Students/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student != null)
            {
                _context.Student.Remove(student);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Student deleted successfully." });
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
    }
}
