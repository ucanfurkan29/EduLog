using EduLog.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduLog.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed roles
            string[] roles = { "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // Seed instructor user
            var instructorEmail = "furkan@edulog.com";
            var existingUser = await userManager.FindByEmailAsync(instructorEmail);
            if (existingUser == null)
            {
                var instructor = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    FullName = "Furkan",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(instructor, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(instructor, "Instructor");
                }
            }
        }
    }
}
