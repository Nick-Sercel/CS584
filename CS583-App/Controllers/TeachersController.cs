﻿using System;
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
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,SubjectId")] Teacher teacher, List<int> SelectedCourseIds)
        {
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
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            ViewBag.Courses = _context.Course.Select(c => new { c.Id, c.Name }).ToList();
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        [Authorize]
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
            ViewBag.Courses = _context.Course.Select(c => new { c.Id, c.Name }).ToList();
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,SubjectId")] Teacher teacher, List<int> SelectedCourseIds)
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subject, "Id", "Name", teacher.SubjectId);
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        [Authorize]
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
        [Authorize]
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
