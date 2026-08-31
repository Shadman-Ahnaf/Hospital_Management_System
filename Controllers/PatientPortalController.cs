using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientPortalController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // PATIENT DASHBOARD
        // URL: /PatientPortal/Dashboard
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                ViewBag.ErrorMessage =
                    "Patient profile could not be found.";

                ViewBag.PatientName = user.FullName;
                ViewBag.TotalAppointments = 0;
                ViewBag.UpcomingAppointments = 0;
                ViewBag.CompletedAppointments = 0;

                return View();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patient.PatientId)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;

            ViewBag.TotalAppointments =
                appointments.Count;

            ViewBag.UpcomingAppointments =
                appointments.Count(a =>
                    a.AppointmentDate.Date >= DateTime.Today &&
                    a.Status == "Scheduled");

            ViewBag.CompletedAppointments =
                appointments.Count(a =>
                    a.Status == "Completed");

            return View();
        }


        // =========================================================
        // MY APPOINTMENTS
        // URL: /PatientPortal/Appointments
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return Forbid();

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;

            return View(appointments);
        }


        // =========================================================
        // MEDICAL RECORDS
        // URL: /PatientPortal/MedicalRecords
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MedicalRecords()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return Forbid();

            var records = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                .Where(m => m.PatientId == patient.PatientId)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;

            return View(records);
        }


        // =========================================================
        // PRESCRIPTIONS
        // URL: /PatientPortal/Prescriptions
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Prescriptions()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return Forbid();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                .Where(p => p.PatientId == patient.PatientId)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;

            return View(prescriptions);
        }
    }
}