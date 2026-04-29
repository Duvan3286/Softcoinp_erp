using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.Infrastructure.Persistence;

public class DbInitializer
{
    public static async Task SeedUsersAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "Counter", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@softcoinp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
