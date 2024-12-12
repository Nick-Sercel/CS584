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
using Microsoft.AspNetCore.Cors;
using System.Text.Json;

namespace CS583_App.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teachers
        [HttpGet]
        [Route("Teachers/Index")]
        public IActionResult Index()
        {
            var applicationDbContext = _context.Teacher.Include(t => t.Field);
            return Json(applicationDbContext);
        }

        // GET: Teachers/Details/5
        [HttpGet]
        [Route("Teachers/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .Include(t => t.Field)
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return Json(teacher);
        }

        // POST: Teachers/Create
        [HttpPost]
        [Route("Teachers/Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var teacher = new Teacher
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

                // Assign selected courses to the teacher
                teacher.Courses = selectedCourses;
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            ViewBag.Courses = _context.Course.Select(c => new { c.Id, c.Name }).ToList();
            return Json(new { success = false, message = "Failed to create teacher" }); 
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [Route("Teachers/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var teacher = new Teacher
            {
                Id = data.GetProperty("Id").GetInt32(),
                Name = data.GetProperty("Name").GetString(),
                Email = data.GetProperty("Email").GetString(),
                SubjectId = data.GetProperty("SubjectId").GetInt32()
            };

            var SelectedCourseIds = data.GetProperty("SelectedCourseIds").EnumerateArray()
                                        .Select(c => c.GetInt32())
                                        .ToList();

            if (id != teacher.Id)
            {
                return NotFound();
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Console.WriteLine(error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    var existingTeacher = await _context.Teacher
                        .Include(c => c.Courses) // Ensure Courses are loaded
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingTeacher != null)
                    {

                        // Fetch the students from the database based on the selected student IDs
                        var selectedCourses = await _context.Course
                            .Where(s => SelectedCourseIds.Contains(s.Id))
                            .ToListAsync();

                        existingTeacher.Courses = selectedCourses;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            return Json(new { success = false, message="Failed to edit teacher" });
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Teachers/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasCourses = _context.Course.Any(t => t.Teacher.Id == id);
            if (hasCourses)
            {
                return Json(new { success = false, message = "Cannot delete a teacher that is associated with a course." });
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher != null)
            {
                _context.Teacher.Remove(teacher);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Teacher deleted successfully." });
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
    }
}
