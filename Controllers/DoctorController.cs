using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // DOCTOR DASHBOARD
        // ==========================================

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                ViewBag.ErrorMessage =
                    "Doctor profile could not be found.";

                return View(new List<Appointment>());
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            ViewBag.DoctorName = doctor.FullName;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.DepartmentName =
                doctor.Department?.DepartmentName ?? "Not Assigned";

            ViewBag.TotalAppointments = appointments.Count;

            ViewBag.TodayAppointments = appointments.Count(a =>
                a.AppointmentDate.Date == DateTime.Today);

            ViewBag.UpcomingAppointments = appointments.Count(a =>
                a.AppointmentDate.Date >= DateTime.Today &&
                a.Status == "Scheduled");

            return View(appointments);
        }


        // ==========================================
        // VIEW APPOINTMENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }


        // ==========================================
        // PATIENT INFORMATION
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> PatientDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            var hasAppointment = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == doctor.DoctorId &&
                    a.PatientId == id);

            if (!hasAppointment)
                return Forbid();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }


        // ==========================================
        // PATIENT MEDICAL HISTORY
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> MedicalHistory(int? patientId)
        {
            if (patientId == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            var hasAppointment = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == doctor.DoctorId &&
                    a.PatientId == patientId);

            if (!hasAppointment)
                return Forbid();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.PatientId == patientId);

            if (patient == null)
                return NotFound();

            var records = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            ViewBag.Patient = patient;

            return View(records);
        }


        // ==========================================
        // CREATE MEDICAL RECORD - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CreateMedicalRecord(int? appointmentId)
        {
            if (appointmentId == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            // Prevent duplicate medical records
            // for the same appointment
            var existingRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(m =>
                    m.AppointmentId == appointment.AppointmentId);

            if (existingRecord != null)
            {
                TempData["ErrorMessage"] =
                    "A medical record already exists for this appointment.";

                return RedirectToAction(
                    nameof(AppointmentDetails),
                    new { id = appointment.AppointmentId });
            }

            var record = new MedicalRecord
            {
                PatientId = appointment.PatientId,
                DoctorId = doctor.DoctorId,
                AppointmentId = appointment.AppointmentId
            };

            ViewBag.PatientName =
                appointment.Patient?.FullName;

            ViewBag.AppointmentDate =
                appointment.AppointmentDate.ToString("dd MMM yyyy");

            return View(record);
        }


        // ==========================================
        // CREATE MEDICAL RECORD - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalRecord(
            MedicalRecord record)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            // Verify appointment belongs to this doctor
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == record.AppointmentId &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return Forbid();

            // Prevent duplicate records
            var existingRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(m =>
                    m.AppointmentId == record.AppointmentId);

            if (existingRecord != null)
            {
                TempData["ErrorMessage"] =
                    "A medical record already exists for this appointment.";

                return RedirectToAction(
                    nameof(AppointmentDetails),
                    new { id = record.AppointmentId });
            }

            // Do not trust submitted DoctorId or PatientId
            record.DoctorId = doctor.DoctorId;
            record.PatientId = appointment.PatientId;
            record.RecordDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                ViewBag.PatientName =
                    appointment.Patient?.FullName;

                ViewBag.AppointmentDate =
                    appointment.AppointmentDate
                        .ToString("dd MMM yyyy");

                return View(record);
            }

            _context.MedicalRecords.Add(record);

            // Consultation completed
            appointment.Status = "Completed";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Medical record created successfully.";

            return RedirectToAction(
                nameof(MedicalHistory),
                new { patientId = record.PatientId });
        }


        // ==========================================
        // CREATE PRESCRIPTION - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CreatePrescription(
            int? medicalRecordId)
        {
            if (medicalRecordId == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m =>
                    m.MedicalRecordId == medicalRecordId &&
                    m.DoctorId == doctor.DoctorId);

            if (medicalRecord == null)
                return NotFound();

            var prescription = new Prescription
            {
                PatientId = medicalRecord.PatientId,
                DoctorId = doctor.DoctorId,
                MedicalRecordId = medicalRecord.MedicalRecordId
            };

            ViewBag.PatientName =
                medicalRecord.Patient?.FullName;

            ViewBag.Diagnosis =
                medicalRecord.Diagnosis;

            return View(prescription);
        }


        // ==========================================
        // CREATE PRESCRIPTION - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePrescription(
            Prescription prescription)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return Forbid();

            // Verify medical record belongs to this doctor
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m =>
                    m.MedicalRecordId ==
                        prescription.MedicalRecordId &&
                    m.DoctorId == doctor.DoctorId);

            if (medicalRecord == null)
                return Forbid();

            // Do not trust submitted DoctorId / PatientId
            prescription.DoctorId = doctor.DoctorId;
            prescription.PatientId = medicalRecord.PatientId;
            prescription.PrescriptionDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                ViewBag.PatientName =
                    medicalRecord.Patient?.FullName;

                ViewBag.Diagnosis =
                    medicalRecord.Diagnosis;

                return View(prescription);
            }

            _context.Prescriptions.Add(prescription);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Prescription created successfully.";

            return RedirectToAction(
                nameof(MedicalHistory),
                new { patientId = prescription.PatientId });
        }
    }
}