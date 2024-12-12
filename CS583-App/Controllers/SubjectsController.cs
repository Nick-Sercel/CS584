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

            return Json(new { success = true, data = subject });
        }

        // POST: Subjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Name")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
            }
            return Json(subject);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subject.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            return Json(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Subject subject)
        {
            if (id != subject.Id)
            {
                return NotFound();
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
                return Json(new { success = true, data=subject });
            }
            return Json(new { success = true, data = subject });
        }

        // GET: Subjects/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Subjects/Delete/5
        [HttpPost]
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
