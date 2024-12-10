using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CS583_App.Data;
using CS583_App.Models;

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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Teacher.Include(t => t.Field);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .Include(t => t.Field)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name");
            ViewBag.Courses = _context.Course.Select(c => new { c.Id, c.Name }).ToList();
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,SubjectId")] Teacher teacher, int[] SelectedCourses)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                foreach (var courseId in SelectedCourses)
                {
                    var course = await _context.Course.FindAsync(courseId);
                    if (course != null)
                    {
                        teacher.Courses.Add(course);
                    }
                }
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            ViewBag.Courses = _context.Course.Select(c => new { c.Id, c.Name }).ToList();
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,SubjectId")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .Include(t => t.Field)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Teachers/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasCourses = _context.Course.Any(t => t.Teacher.Id == id);
            if (hasCourses)
            {
                return Json(new { success = false, message = "Cannot delete a teacer that is associated with a course." });
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
