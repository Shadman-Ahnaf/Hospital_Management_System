using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }


        // ==========================================
        // LOGIN
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            string selectedRole)
        {
            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                ModelState.AddModelError(
                    "",
                    "Please select how you want to login.");

                return View();
            }

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }

            string actualRole = "";

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                actualRole = "Admin";
            }
            else if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                actualRole = "Doctor";
            }
            else if (await _userManager.IsInRoleAsync(user, "Receptionist"))
            {
                actualRole = "Receptionist";
            }
            else if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                actualRole = "Patient";
            }

            if (actualRole != selectedRole)
            {
                ModelState.AddModelError(
                    "",
                    "The selected login type does not match this account.");

                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                false,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }


            // ==========================================
            // FORCE TEMPORARY PASSWORD CHANGE
            // ==========================================

            if (user.MustChangePassword)
            {
                if (actualRole == "Doctor" ||
                    actualRole == "Receptionist" ||
                    actualRole == "Patient")
                {
                    return RedirectToAction(
                        nameof(ForceChangePassword),
                        "Account");
                }
            }


            // ==========================================
            // ROLE DASHBOARD REDIRECTION
            // ==========================================

            if (actualRole == "Admin")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Administrator");
            }

            if (actualRole == "Doctor")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Doctor");
            }

            if (actualRole == "Receptionist")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Receptionist");
            }

            if (actualRole == "Patient")
            {
                return RedirectToAction(
                    "Dashboard",
                    "PatientPortal");
            }


            // No valid role
            await _signInManager.SignOutAsync();

            ModelState.AddModelError(
                "",
                "This account does not have a valid HMS role.");

            return View();
        }


        // ==========================================
        // PROFILE
        // ==========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            ViewBag.Role = "User";


            // ==========================================
            // ADMINISTRATOR
            // ==========================================

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ViewBag.Role = "Administrator";
            }


            // ==========================================
            // DOCTOR
            // ==========================================

            else if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                ViewBag.Role = "Doctor";

                var doctor = await _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor != null)
                {
                    ViewBag.DoctorName = doctor.FullName;
                    ViewBag.Specialization = doctor.Specialization;
                    ViewBag.DoctorPhone = doctor.Phone;

                    ViewBag.DepartmentName =
                        doctor.Department?.DepartmentName ?? "Not assigned";

                    ViewBag.IsAvailable = doctor.IsAvailable;
                }
            }


            // ==========================================
            // RECEPTIONIST
            // ==========================================

            else if (await _userManager.IsInRoleAsync(user, "Receptionist"))
            {
                ViewBag.Role = "Receptionist";

                var receptionist = await _context.Receptionists
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (receptionist != null)
                {
                    ViewBag.ReceptionistName = receptionist.FullName;
                    ViewBag.ReceptionistEmail = receptionist.Email;
                    ViewBag.ReceptionistPhone = receptionist.Phone;
                    ViewBag.IsActive = receptionist.IsActive;
                }
            }


            // ==========================================
            // PATIENT
            // ==========================================

            else if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                ViewBag.Role = "Patient";

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (patient != null)
                {
                    ViewBag.PatientName = patient.FullName;

                    ViewBag.DateOfBirth =
                        patient.DateOfBirth.ToString("dd MMM yyyy");

                    ViewBag.Gender = patient.Gender;
                    ViewBag.PatientPhone = patient.Phone;
                    ViewBag.BloodGroup = patient.BloodGroup;
                    ViewBag.EmergencyContact = patient.EmergencyContact;
                    ViewBag.Address = patient.Address;
                }
            }

            return View(user);
        }


        // ==========================================
        // EDIT PROFILE - GET
        // ==========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            return View(user);
        }


        // ==========================================
        // EDIT PROFILE - POST
        // ==========================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            string fullName,
            string? phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));


            // ==========================================
            // VALIDATE FULL NAME
            // ==========================================

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "Full name is required.");

                return View(user);
            }

            if (fullName.Length > 100)
            {
                ModelState.AddModelError(
                    "FullName",
                    "Full name cannot exceed 100 characters.");

                return View(user);
            }


            // ==========================================
            // UPDATE ASP.NET IDENTITY USER
            // ==========================================

            user.FullName = fullName.Trim();

            user.PhoneNumber =
                string.IsNullOrWhiteSpace(phoneNumber)
                    ? null
                    : phoneNumber.Trim();


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(user);
            }


            // ==========================================
            // UPDATE DOCTOR PROFILE
            // ==========================================

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor != null)
                {
                    doctor.FullName = user.FullName;

                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        doctor.Phone = phoneNumber.Trim();
                    }

                    await _context.SaveChangesAsync();
                }
            }


            // ==========================================
            // UPDATE PATIENT PROFILE
            // ==========================================

            else if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (patient != null)
                {
                    patient.FullName = user.FullName;

                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        patient.Phone = phoneNumber.Trim();
                    }

                    await _context.SaveChangesAsync();
                }
            }


            // ==========================================
            // UPDATE RECEPTIONIST PROFILE
            // ==========================================

            else if (await _userManager.IsInRoleAsync(user, "Receptionist"))
            {
                var receptionist = await _context.Receptionists
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (receptionist != null)
                {
                    receptionist.FullName = user.FullName;

                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        receptionist.Phone = phoneNumber.Trim();
                    }

                    await _context.SaveChangesAsync();
                }
            }


            // Refresh authentication cookie
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Your profile has been updated successfully.";

            return RedirectToAction(nameof(Profile));
        }


        // ==========================================
        // SETTINGS
        // ==========================================

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }


        // ==========================================
        // NORMAL CHANGE PASSWORD
        // Used from:
        // Settings -> Change Password
        //
        // This is DIFFERENT from ForceChangePassword.
        // ==========================================

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        // ==========================================
        // NORMAL CHANGE PASSWORD - POST
        //
        // Requires:
        // 1. Current password
        // 2. New password
        // 3. Confirm new password
        //
        // Does NOT automatically modify
        // MustChangePassword.
        // ==========================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(
                    "",
                    "All password fields are required.");

                return View();
            }


            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "New passwords do not match.");

                return View();
            }


            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));


            // ==========================================
            // VERIFY CURRENT PASSWORD AND CHANGE IT
            // ==========================================

            var result = await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View();
            }


            // ==========================================
            // IMPORTANT:
            //
            // Do NOT set:
            //
            // user.MustChangePassword = false;
            //
            // because this page is for normal password
            // changes, not temporary-password activation.
            // ==========================================

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Your password has been changed successfully.";


            // ==========================================
            // RETURN TO PROFILE
            // ==========================================

            return RedirectToAction(nameof(Profile));
        }


        // ==========================================
        // FORCE CHANGE PASSWORD
        //
        // Used ONLY when an account has:
        //
        // MustChangePassword == true
        //
        // This is the temporary password flow.
        // ==========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ForceChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));


            // If this user does not require a forced
            // password change, don't allow them to
            // access this page directly.

            if (!user.MustChangePassword)
            {
                return RedirectToAction(nameof(Profile));
            }

            return View();
        }


        // ==========================================
        // FORCE CHANGE PASSWORD - POST
        //
        // Used after first login with temporary password.
        //
        // Requires:
        // 1. Temporary/current password
        // 2. New password
        // 3. Confirm new password
        //
        // After successful change:
        // MustChangePassword = false
        // ==========================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));


            // ==========================================
            // SECURITY CHECK
            // ==========================================

            if (!user.MustChangePassword)
            {
                return RedirectToAction(nameof(Profile));
            }


            // ==========================================
            // VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(
                    "",
                    "All password fields are required.");

                return View();
            }


            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "New passwords do not match.");

                return View();
            }


            // ==========================================
            // CHANGE TEMPORARY PASSWORD
            // ==========================================

            var result = await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View();
            }


            // ==========================================
            // TEMPORARY PASSWORD NO LONGER VALID
            // ==========================================

            user.MustChangePassword = false;

            var updateResult =
                await _userManager.UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View();
            }


            // Refresh login session
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Your password has been changed successfully.";


            // ==========================================
            // REDIRECT TO CORRECT DASHBOARD
            // ==========================================

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Administrator");
            }

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Doctor");
            }

            if (await _userManager.IsInRoleAsync(user, "Receptionist"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Receptionist");
            }

            if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "PatientPortal");
            }

            return RedirectToAction(
                "Index",
                "Home");
        }


        // ==========================================
        // LOGOUT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}