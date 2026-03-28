using Microsoft.AspNetCore.Identity;
using AspNetMvcProject.Data;
using AspNetMvcProject.Models;
using AspNetMvcProject.Utility;
using Microsoft.EntityFrameworkCore;

namespace AspNetMvcProject.Utility;

public static class DbInitializer
{
    public static void Seed(IApplicationBuilder applicationBuilder)
    {
        using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Database.EnsureCreated();

            if (!context.Users.Any(u => u.Username == "admintest"))
            {
                var hasher = new PasswordHasher<ApplicationUser>();
                var admin = new ApplicationUser
                {
                    Username = "admintest",
                    Email = "admin@test.com",
                    FullName = "Admin Test Account",
                    Role = SD.Role_Admin,
                    Address = "System Generated",
                    Age = 99
                };
                admin.PasswordHash = hasher.HashPassword(admin, "hoadzdz");

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
