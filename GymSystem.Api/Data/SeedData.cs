using Microsoft.AspNetCore.Identity;
using GymSystem.Api.Models;

namespace GymSystem.Api.Data
{
    public static class SeedData
    {
        public static async Task EnsureRolesAndAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            string[] roles = ["Admin", "Staff", "Trainer", "Member"];

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
                var password = config["SeedAdmin:Password"];

                var result = await userManager.CreateAsync(adminUser, password); // change password in production
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}