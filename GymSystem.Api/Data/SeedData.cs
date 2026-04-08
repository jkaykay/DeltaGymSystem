// ============================================================
// SeedData.cs — Database seeding.
// This class runs once on application startup to ensure the
// required roles exist and a default admin account is created.
// Without seeding, there would be no way to log in as Admin
// the first time the app is deployed.
// ============================================================

using Microsoft.AspNetCore.Identity;
using GymSystem.Api.Models;

namespace GymSystem.Api.Data
{
    public static class SeedData
    {
        // This method is called from Program.cs at startup.
        // It creates the four application roles and an initial admin user.
        public static async Task EnsureRolesAndAdminAsync(IServiceProvider services)
        {
            // Create a scoped service container so we can resolve
            // Identity services (RoleManager, UserManager) and configuration.
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // The four roles used throughout the application.
            string[] roles = ["Admin", "Staff", "Trainer", "Member"];

            // Create each role if it doesn't already exist in the database.
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Optional: create a default admin account if none exists
            var adminEmail = "admin@localhost";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    JoinDate = DateTime.UtcNow,
                    Active = true
                };
                // Read the admin password from configuration (user-secrets in dev).
                var password = config["SeedAdmin:Password"];

                var result = await userManager.CreateAsync(adminUser, password); // change password in production
                if (result.Succeeded)
                {
                    // Assign the Admin role so this user has full access.
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}