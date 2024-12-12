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
using System.Text.Json;

namespace CS583_App.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Subjects
        [HttpGet]
        [Route("Subjects/Index")]
        public IActionResult Index()
        {
            return Json(_context.Subject);
        }

        // GET: Subjects/Details/5
        [HttpGet]
        [Route("Subjects/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subject
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return Json(subject);
        }

        // POST: Subjects/Create
        [HttpPost]
        [Route("Subjects/Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var subject = new Subject
            {
                Id = 0,
                Name = data.GetProperty("Name").GetString(),
            };

            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to create subject" });
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [Route("Subjects/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var subject = new Subject
            {
                Id = data.GetProperty("Id").GetInt32(),
                Name = data.GetProperty("Name").GetString(),
            };

            if (id != subject.Id)
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
                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Json(new { success = false});
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to edit subject" });
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Subjects/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasTeachers = _context.Teacher.Any(t => t.SubjectId == id);
            var hasStudents = _context.Student.Any(t => t.SubjectId == id);
            var hasCourses = _context.Course.Any(c => c.SubjectId == id);
            if (hasTeachers || hasStudents || hasCourses)
            {
                return Json(new { success = false, message = "Cannot delete a subject that is associated with something." });
            }

            var subject = await _context.Subject.FindAsync(id);
            if (subject != null)
            {
                _context.Subject.Remove(subject);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Subject deleted successfully." });
        }

        private bool SubjectExists(int id)
        {
            return _context.Subject.Any(e => e.Id == id);
        }
    }
}
