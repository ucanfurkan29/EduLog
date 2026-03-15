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

            // Seed student users
            var students = new[]
            {
                new { Email = "ogrenci@edulog.com", FullName = "Öğrenci Demo" },
                new { Email = "ogrenci1@edulog.com", FullName = "Ali Yılmaz" },
                new { Email = "ogrenci2@edulog.com", FullName = "Ayşe Demir" },
                new { Email = "ogrenci3@edulog.com", FullName = "Mehmet Kaya" },
                new { Email = "ogrenci4@edulog.com", FullName = "Zeynep Çelik" },
            };

            foreach (var s in students)
            {
                if (await userManager.FindByEmailAsync(s.Email) == null)
                {
                    var student = new ApplicationUser
                    {
                        UserName = s.Email,
                        Email = s.Email,
                        FullName = s.FullName,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(student, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(student, "Student");
                    }
                }
            }
        }
    }
}
