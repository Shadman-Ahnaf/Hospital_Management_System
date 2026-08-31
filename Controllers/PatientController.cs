using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // PATIENT MANAGEMENT
        // =========================================================

        public async Task<IActionResult> Index(string? search)
        {
            var patients = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                patients = patients.Where(p =>
                    p.FullName.Contains(search) ||
                    p.Phone.Contains(search) ||
                    (p.Email != null && p.Email.Contains(search)));
            }

            ViewBag.Search = search;

            return View(await patients
                .OrderBy(p => p.FullName)
                .ToListAsync());
        }

        // =========================================================
        // PATIENT DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // =========================================================
        // PATIENT SEARCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Search(string? search)
        {
            var patients = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                patients = patients.Where(p =>
                    p.FullName.Contains(search) ||
                    p.Phone.Contains(search) ||
                    (p.Email != null && p.Email.Contains(search)));
            }

            ViewBag.Search = search;

            return View(await patients
                .OrderBy(p => p.FullName)
                .ToListAsync());
        }

        // =========================================================
        // SEARCH DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SearchDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // =========================================================
        // CREATE PATIENT
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (!ModelState.IsValid)
                return View(patient);

            // Generate temporary password
            string temporaryPassword =
                Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(9))
                .Replace("+", "A")
                .Replace("/", "B")
                .Replace("=", "C");

            // Create patient record first
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Create Identity account if email exists
            if (!string.IsNullOrWhiteSpace(patient.Email))
            {
                var existingUser =
                    await _userManager.FindByEmailAsync(patient.Email);

                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = patient.Email,
                        Email = patient.Email,
                        FullName = patient.FullName,
                        EmailConfirmed = true,
                        MustChangePassword = true
                    };

                    var result =
                        await _userManager.CreateAsync(
                            user,
                            temporaryPassword);

                    if (result.Succeeded)
                    {
                        // IMPORTANT:
                        // Link the Patient record to the Identity user
                        patient.UserId = user.Id;

                        _context.Patients.Update(patient);
                        await _context.SaveChangesAsync();

                        await _userManager.AddToRoleAsync(
                            user,
                            "Patient");

                        TempData["TemporaryCredentials"] =
                            $"Patient account created. Email: {patient.Email} | Temporary Password: {temporaryPassword}";
                    }
                    else
                    {
                        TempData["ErrorMessage"] =
                            "Patient was registered, but the patient account could not be created.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Patient was registered, but an account with this email already exists.";
                }
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Patient was registered without a login account because no email was provided.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT PATIENT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Patient patient)
        {
            if (id != patient.PatientId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(patient);

            _context.Patients.Update(patient);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Patient information updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE PATIENT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.PatientId == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients
                .FindAsync(id);

            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Patient removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}