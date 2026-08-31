using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // STAFF LIST
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            var receptionists = await _context.Receptionists
                .OrderBy(r => r.FullName)
                .ToListAsync();

            ViewBag.Doctors = doctors;
            ViewBag.Receptionists = receptionists;

            return View();
        }

        // ==========================================
        // CREATE DOCTOR - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View();
        }

        // ==========================================
        // CREATE DOCTOR - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(
            Doctor doctor,
            string temporaryPassword)
        {
            if (string.IsNullOrWhiteSpace(doctor.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email is required for a Doctor account.");
            }

            if (string.IsNullOrWhiteSpace(temporaryPassword))
            {
                ModelState.AddModelError(
                    "",
                    "Temporary password is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(doctor);
            }

            // Check existing Identity account
            var existingUser = await _userManager
                .FindByEmailAsync(doctor.Email!);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(doctor);
            }

            // Create Identity account
            var user = new ApplicationUser
            {
                UserName = doctor.Email,
                Email = doctor.Email,
                FullName = doctor.FullName,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(
                user,
                temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(doctor);
            }

            // Assign Doctor role
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Doctor");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(doctor);
            }

            // Link Doctor profile to Identity account
            doctor.UserId = user.Id;

            _context.Doctors.Add(doctor);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Doctor account created successfully for {doctor.FullName}.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CREATE RECEPTIONIST - GET
        // ==========================================

        [HttpGet]
        public IActionResult CreateReceptionist()
        {
            return View();
        }

        // ==========================================
        // CREATE RECEPTIONIST - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceptionist(
            Receptionist receptionist,
            string temporaryPassword)
        {
            if (string.IsNullOrWhiteSpace(temporaryPassword))
            {
                ModelState.AddModelError(
                    "",
                    "Temporary password is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(receptionist);
            }

            // Check existing Identity account
            var existingUser = await _userManager
                .FindByEmailAsync(receptionist.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                return View(receptionist);
            }

            // Create Identity account
            var user = new ApplicationUser
            {
                UserName = receptionist.Email,
                Email = receptionist.Email,
                FullName = receptionist.FullName,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(
                user,
                temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(receptionist);
            }

            // Assign Receptionist role
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Receptionist");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(receptionist);
            }

            // Link Receptionist profile to Identity account
            receptionist.UserId = user.Id;

            _context.Receptionists.Add(receptionist);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Receptionist account created successfully for {receptionist.FullName}.";

            return RedirectToAction(nameof(Index));
        }
    }
}