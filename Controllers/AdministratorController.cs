using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdministratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADMINISTRATOR DASHBOARD
        // =========================================================

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalPatients =
                await _context.Patients.CountAsync();

            ViewBag.TotalDoctors =
                await _context.Doctors.CountAsync();

            ViewBag.TotalReceptionists =
                await _context.Receptionists.CountAsync();

            ViewBag.TotalDepartments =
                await _context.Departments.CountAsync();

            ViewBag.TotalAppointments =
                await _context.Appointments.CountAsync();

            return View();
        }


        // =========================================================
        // ADMIN REPORTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            // -----------------------------------------------------
            // GENERAL STATISTICS
            // -----------------------------------------------------

            ViewBag.TotalPatients =
                await _context.Patients.CountAsync();

            ViewBag.TotalDoctors =
                await _context.Doctors.CountAsync();

            ViewBag.TotalReceptionists =
                await _context.Receptionists.CountAsync();

            ViewBag.TotalDepartments =
                await _context.Departments.CountAsync();

            ViewBag.TotalAppointments =
                await _context.Appointments.CountAsync();

            ViewBag.TotalMedicalRecords =
                await _context.MedicalRecords.CountAsync();

            ViewBag.TotalPrescriptions =
                await _context.Prescriptions.CountAsync();


            // -----------------------------------------------------
            // APPOINTMENT STATUS
            // -----------------------------------------------------

            ViewBag.ScheduledAppointments =
                await _context.Appointments
                    .CountAsync(a => a.Status == "Scheduled");

            ViewBag.CompletedAppointments =
                await _context.Appointments
                    .CountAsync(a => a.Status == "Completed");

            ViewBag.CancelledAppointments =
                await _context.Appointments
                    .CountAsync(a => a.Status == "Cancelled");


            // -----------------------------------------------------
            // TODAY'S APPOINTMENTS
            // -----------------------------------------------------

            ViewBag.TodayAppointments =
                await _context.Appointments
                    .CountAsync(a =>
                        a.AppointmentDate.Date ==
                        DateTime.Today);


            // -----------------------------------------------------
            // DEPARTMENT-WISE DOCTOR COUNT
            // -----------------------------------------------------

            var departmentReports =
                await _context.Departments
                    .Select(d => new
                    {
                        DepartmentName = d.DepartmentName,

                        DoctorCount = _context.Doctors
                            .Count(doc =>
                                doc.DepartmentId ==
                                d.DepartmentId)
                    })
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

            ViewBag.DepartmentReports =
                departmentReports;


            // -----------------------------------------------------
            // RECENT APPOINTMENTS
            // -----------------------------------------------------

            var recentAppointments =
                await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Take(10)
                    .ToListAsync();

            ViewBag.RecentAppointments =
                recentAppointments;


            return View();
        }
    }
}