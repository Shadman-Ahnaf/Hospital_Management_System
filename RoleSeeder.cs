using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Admin",
                "Doctor",
                "Receptionist",
                "Patient"
            };

            // Create roles
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create demo users
            await CreateUserAsync(
                userManager,
                "admin@hms.com",
                "Admin@123",
                "HMS Administrator",
                "Admin");

            await CreateUserAsync(
                userManager,
                "doctor@hms.com",
                "Doctor@123",
                "Dr. Demo Doctor",
                "Doctor");

            await CreateUserAsync(
                userManager,
                "receptionist@hms.com",
                "Reception@123",
                "Demo Receptionist",
                "Receptionist");

            await CreateUserAsync(
                userManager,
                "patient@hms.com",
                "Patient@123",
                "Demo Patient",
                "Patient");
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}