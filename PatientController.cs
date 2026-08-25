using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Patient List + Search
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

        // Patient Details
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

        // Create Patient
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

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient registered successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Edit Patient
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
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.PatientId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(patient);

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient information updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Delete Patient
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
