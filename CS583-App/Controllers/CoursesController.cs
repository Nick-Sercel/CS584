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

            return View(course);
        }

        // GET: Courses/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name");
            ViewBag.Students = _context.Student.Select(c => new { c.Id, c.Name }).ToList();
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,TeacherId,SubjectId")] Course course, List<int> SelectedStudentIds)
        {
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", course.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name", course.TeacherId);
            ViewBag.Students = await _context.Student.ToListAsync();
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", course.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name", course.TeacherId);
            ViewBag.Students = _context.Student.Select(c => new { c.Id, c.Name }).ToList();
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,TeacherId,SubjectId")] Course course, List<int> SelectedStudentIds)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            System.Console.WriteLine("Student IDs");
            System.Console.WriteLine(SelectedStudentIds.Count);
            System.Console.WriteLine("End Student IDs");

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
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", course.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teacher, "Id", "Name", course.TeacherId);
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Subject)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course != null)
            {
                _context.Course.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
