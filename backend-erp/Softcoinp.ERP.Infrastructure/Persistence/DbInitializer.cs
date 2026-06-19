using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Infrastructure.Persistence;

public class DbInitializer
{
    /// <summary>
    /// Siembra los 6 roles del negocio y el usuario SuperAdmin inicial.
    /// </summary>
    public static async Task SeedUsersAsync(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration? configuration = null)
    {
        // 1. Sembrar los 6 roles del negocio (reemplaza Admin/Counter/Viewer)
        var roles = Enum.GetNames<AppRole>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Sembrar usuario SuperAdmin
        var adminEmail = configuration?["SeedData:AdminEmail"] ?? "superadmin@dev";
        var adminPassword = configuration?["SeedData:AdminPassword"] ?? "SuperDev2026!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Super Administrator",
                EmailConfirmed = true,
                IsActive = true,
                IsSuspended = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, nameof(AppRole.SuperAdmin));
            }
        }
        else
        {
            // En desarrollo: garantizar que la contraseña sea siempre la del config
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

            // Asegurar que tenga el rol correcto
            var currentRoles = await userManager.GetRolesAsync(adminUser);
            if (!currentRoles.Contains(nameof(AppRole.SuperAdmin)))
            {
                await userManager.RemoveFromRolesAsync(adminUser, currentRoles);
                await userManager.AddToRoleAsync(adminUser, nameof(AppRole.SuperAdmin));
            }
        }
    }
}
