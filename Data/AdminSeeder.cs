using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<ApplicationUser>>();

            var existingAdmin =
                await userManager.FindByEmailAsync(
                    "admin@hms.com");

            if (existingAdmin != null)
                return;

            var admin = new ApplicationUser
            {
                UserName = "admin@hms.com",
                Email = "admin@hms.com",
                FullName = "HMS Administrator",
                EmailConfirmed = true,
                MustChangePassword = false
            };

            var result = await userManager.CreateAsync(
                admin,
                "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }
        }
    }
}