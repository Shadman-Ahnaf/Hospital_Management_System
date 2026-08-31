using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // DEPARTMENT LIST
        // =========================================================

        public async Task<IActionResult> Index(string? search)
        {
            var departments = _context.Departments
                .Include(d => d.Doctors)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                departments = departments.Where(d =>
                    d.DepartmentName.Contains(search) ||
                    (d.Description != null &&
                     d.Description.Contains(search)) ||
                    (d.Location != null &&
                     d.Location.Contains(search)));
            }

            ViewBag.Search = search;

            return View(await departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync());
        }


        // =========================================================
        // DEPARTMENT DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d =>
                    d.DepartmentId == id);

            if (department == null)
                return NotFound();

            return View(department);
        }


        // =========================================================
        // CREATE DEPARTMENT
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Department department)
        {
            if (!ModelState.IsValid)
                return View(department);

            bool exists = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentName.ToLower() ==
                    department.DepartmentName.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    "DepartmentName",
                    "A department with this name already exists.");

                return View(department);
            }

            _context.Departments.Add(department);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Department created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT DEPARTMENT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var department = await _context.Departments
                .FindAsync(id);

            if (department == null)
                return NotFound();

            return View(department);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Department department)
        {
            if (id != department.DepartmentId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(department);

            bool exists = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentId != id &&
                    d.DepartmentName.ToLower() ==
                    department.DepartmentName.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    "DepartmentName",
                    "A department with this name already exists.");

                return View(department);
            }

            var existingDepartment =
                await _context.Departments.FindAsync(id);

            if (existingDepartment == null)
                return NotFound();

            existingDepartment.DepartmentName =
                department.DepartmentName;

            existingDepartment.Description =
                department.Description;

            existingDepartment.Location =
                department.Location;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Department updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE DEPARTMENT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d =>
                    d.DepartmentId == id);

            if (department == null)
                return NotFound();

            return View(department);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d =>
                    d.DepartmentId == id);

            if (department == null)
                return NotFound();

            // Prevent deletion if doctors are assigned
            if (department.Doctors.Any())
            {
                TempData["ErrorMessage"] =
                    "This department cannot be deleted because doctors are currently assigned to it.";

                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Department deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}