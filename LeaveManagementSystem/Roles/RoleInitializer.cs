using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Data;
using System.Threading.Tasks;

namespace LeaveManagementSystem.Roles
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                await context.Database.MigrateAsync();

                // Create roles
                string[] roleNames = { "Admin", "Employee" };

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Create default admin user
                var adminEmail = "admin@technova.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    string adminPassword = "Admin@123";
                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}