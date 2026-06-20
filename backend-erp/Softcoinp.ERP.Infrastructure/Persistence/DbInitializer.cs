using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    /// <summary>
    /// Siembra el plan de cuentas estándar oficial de la Resolución 029 para el tenant.
    /// </summary>
    public static async Task SeedChartOfAccountsAsync(ApplicationDbContext context, string tenantId)
    {
        var hasAccounts = await context.AccountingAccounts.AnyAsync(a => a.TenantId == tenantId);
        if (hasAccounts)
        {
            return;
        }

        var standardAccounts = new List<AccountingAccount>
        {
            // --- 1. ACTIVOS (Debit) ---
            new AccountingAccount { Code = "1", Name = "Activo", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "11", Name = "Efectivo y Equivalentes de Efectivo", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "1105", Name = "Caja", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "1110", Name = "Bancos", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "111001", Name = "Cuenta Bancaria Principal", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = false, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "13", Name = "Cuentas por Cobrar", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "1305", Name = "Cuotas de Administración", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "1355", Name = "Otras Cuentas por Cobrar", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "14", Name = "Inventarios", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "15", Name = "Propiedades, Planta y Equipo", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "1524", Name = "Maquinaria y Equipo", Category = AccountingAccountCategory.Asset, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },

            // --- 2. PASIVOS (Credit) ---
            new AccountingAccount { Code = "2", Name = "Pasivo", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "22", Name = "Proveedores", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "23", Name = "Cuentas por Pagar", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "2335", Name = "Costos y Gastos por Pagar", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "2380", Name = "Acreedores Varios", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "25", Name = "Obligaciones Laborales", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "2505", Name = "Salarios por Pagar", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "28", Name = "Otros Pasivos", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "2805", Name = "Anticipos Recibidos", Category = AccountingAccountCategory.Liability, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },

            // --- 3. PATRIMONIO / FONDO SOCIAL (Credit) ---
            new AccountingAccount { Code = "3", Name = "Patrimonio", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "31", Name = "Fondo Social", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "3105", Name = "Fondo Social Efectivo", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "32", Name = "Reservas (Fondo de Imprevistos)", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "3205", Name = "Fondo de Imprevistos Ley 675", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = false, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "37", Name = "Resultados Acumulados", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "3705", Name = "Resultados de Ejercicios Anteriores", Category = AccountingAccountCategory.Equity, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },

            // --- 4. INGRESOS (Credit) ---
            new AccountingAccount { Code = "4", Name = "Ingresos", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "41", Name = "Ingresos Ordinarios", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4105", Name = "Cuotas de Administración Ordinarias", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4110", Name = "Cuotas de Administración Extraordinarias", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4195", Name = "Otros Ingresos", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "42", Name = "Ingresos No Operacionales", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4210", Name = "Rendimientos Financieros", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4220", Name = "Arrendamiento Zonas Comunes", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "4230", Name = "Multas y Sanciones", Category = AccountingAccountCategory.Income, Nature = AccountingAccountNature.Credit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },

            // --- 5. GASTOS (Debit) ---
            new AccountingAccount { Code = "5", Name = "Gastos", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "51", Name = "Gastos de Administración", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5105", Name = "Gastos de Personal", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5110", Name = "Honorarios", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5130", Name = "Seguros Áreas Comunes", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5135", Name = "Servicios Públicos", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5140", Name = "Vigilancia y Seguridad", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5145", Name = "Mantenimiento y Conservación", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5195", Name = "Gastos Diversos", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = true, IsOfficialStandard = true, TenantId = tenantId },
            new AccountingAccount { Code = "5196", Name = "Aporte Fondo de Imprevistos", Category = AccountingAccountCategory.Expense, Nature = AccountingAccountNature.Debit, IsGroup = false, IsOfficialStandard = true, TenantId = tenantId }
        };

        context.AccountingAccounts.AddRange(standardAccounts);
        await context.SaveChangesAsync();
    }
}
