using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // APPOINTMENT LIST
        // ==========================================

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            DateTime? date)
        {
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            // Doctor can see ONLY their own appointments
            if (User.IsInRole("Doctor"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Forbid();

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor == null)
                    return Forbid();

                appointments = appointments
                    .Where(a => a.DoctorId == doctor.DoctorId);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                appointments = appointments.Where(a =>
                    a.Patient!.FullName.Contains(search) ||
                    a.Doctor!.FullName.Contains(search));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                appointments = appointments
                    .Where(a => a.Status == status);
            }

            // Date filter
            if (date.HasValue)
            {
                appointments = appointments
                    .Where(a => a.AppointmentDate == date.Value.Date);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(await appointments
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync());
        }


        // ==========================================
        // APPOINTMENT DETAILS
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            // Doctor can see ONLY their own appointment
            if (User.IsInRole("Doctor"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Forbid();

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor == null)
                    return Forbid();

                if (appointment.DoctorId != doctor.DoctorId)
                    return Forbid();
            }

            return View(appointment);
        }


        // ==========================================
        // CREATE APPOINTMENT
        // ADMIN + RECEPTIONIST ONLY
        // ==========================================

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadAppointmentDropdowns();

            return View();
        }


        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            // Check doctor exists and is available
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == appointment.DoctorId &&
                    d.IsAvailable);

            if (doctor == null)
            {
                ModelState.AddModelError(
                    "DoctorId",
                    "The selected doctor is not available.");

                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            // Prevent double booking
            bool alreadyBooked =
                await _context.Appointments.AnyAsync(a =>
                    a.DoctorId == appointment.DoctorId &&
                    a.AppointmentDate ==
                        appointment.AppointmentDate &&
                    a.AppointmentTime ==
                        appointment.AppointmentTime &&
                    a.Status != "Cancelled");

            if (alreadyBooked)
            {
                ModelState.AddModelError(
                    "",
                    "The selected doctor already has an appointment at this time.");

                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            appointment.Status = "Scheduled";

            _context.Appointments.Add(appointment);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Appointment scheduled successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // EDIT APPOINTMENT
        // ADMIN + RECEPTIONIST ONLY
        // ==========================================

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .FindAsync(id);

            if (appointment == null)
                return NotFound();

            await LoadAppointmentDropdowns();

            return View(appointment);
        }


        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Appointment appointment)
        {
            if (id != appointment.AppointmentId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            // Check doctor exists and is available
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == appointment.DoctorId &&
                    d.IsAvailable);

            if (doctor == null)
            {
                ModelState.AddModelError(
                    "DoctorId",
                    "The selected doctor is not available.");

                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            // Prevent double booking during edit
            bool alreadyBooked =
                await _context.Appointments.AnyAsync(a =>
                    a.AppointmentId != appointment.AppointmentId &&
                    a.DoctorId == appointment.DoctorId &&
                    a.AppointmentDate ==
                        appointment.AppointmentDate &&
                    a.AppointmentTime ==
                        appointment.AppointmentTime &&
                    a.Status != "Cancelled");

            if (alreadyBooked)
            {
                ModelState.AddModelError(
                    "",
                    "The selected doctor already has an appointment at this time.");

                await LoadAppointmentDropdowns();

                return View(appointment);
            }

            _context.Appointments.Update(appointment);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Appointment updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // CANCEL APPOINTMENT
        // ADMIN + RECEPTIONIST ONLY
        // ==========================================

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            // Do not cancel an already completed appointment
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] =
                    "A completed appointment cannot be cancelled.";

                return RedirectToAction(nameof(Index));
            }

            // Do not cancel an already cancelled appointment
            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "This appointment is already cancelled.";

                return RedirectToAction(nameof(Index));
            }

            // Keep appointment history.
            // Only change its status.
            appointment.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Appointment cancelled successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // LOAD PATIENT + DOCTOR DROPDOWNS
        // ==========================================

        private async Task LoadAppointmentDropdowns()
        {
            ViewBag.Patients = await _context.Patients
                .OrderBy(p => p.FullName)
                .ToListAsync();

            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.IsAvailable)
                .OrderBy(d => d.FullName)
                .ToListAsync();
        }
    }
}