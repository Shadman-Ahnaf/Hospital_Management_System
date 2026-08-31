using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Admin",
                "Doctor",
                "Receptionist",
                "Patient"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}