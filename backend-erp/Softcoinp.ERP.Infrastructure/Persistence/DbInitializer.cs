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
    /// Siembra los tipos de reporte estándar y sus plantillas PDF por defecto.
    /// </summary>
    public static async Task SeedReportTypesAsync(ApplicationDbContext context, string tenantId)
    {
        var hasReportTypes = await context.ReportTypes.AnyAsync(r => r.TenantId == tenantId);
        if (hasReportTypes)
        {
            return;
        }

        var reports = new List<ReportType>
        {
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.BudgetExecution,
                Name = "Ejecución Presupuestal", Description = "Comparación entre el presupuesto aprobado y los gastos/ingresos reales ejecutados en el período.",
                Category = ReportCategory.Financial, SourceModules = "Budgets", AllowedRoles = "SuperAdmin,Admin,Council,Accountant,Auditor",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.ContingencyFund,
                Name = "Fondo de Imprevistos", Description = "Detalle de movimientos, saldo actual y proyección del fondo de imprevistos de Ley 675.",
                Category = ReportCategory.Financial, SourceModules = "ContingencyFund", AllowedRoles = "SuperAdmin,Admin,Council,Accountant,Auditor",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.CashFlow,
                Name = "Flujo de Caja", Description = "Estado de flujos de efectivo que muestra las entradas y salidas de dinero operativas, de inversión y de financiación.",
                Category = ReportCategory.Financial,                 SourceModules = "Billing", AllowedRoles = "SuperAdmin,Admin,Council,Accountant,Auditor",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PortfolioAging,
                Name = "Cartera por Edades", Description = "Análisis de cartera morosa clasificada por rangos de antigüedad: 0-30, 31-60, 61-90 y más de 90 días.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio", AllowedRoles = "SuperAdmin,Admin,Council,Accountant,Auditor",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PortfolioByUnit,
                Name = "Cartera por Unidad", Description = "Estado de cuenta individual por unidad privada con detalle de cuotas pagadas y pendientes.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio,Units", AllowedRoles = "SuperAdmin,Admin,Council,Resident",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.TopDebtors,
                Name = "Principales Deudores", Description = "Ranking de los propietarios con mayor saldo pendiente de pago.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PeriodCollection,
                Name = "Recaudo del Período", Description = "Resumen de pagos recibidos en el período, incluyendo forma de pago y unidad.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio", AllowedRoles = "SuperAdmin,Admin,Council,Accountant,Auditor",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PortfolioProjection,
                Name = "Proyección de Cartera", Description = "Proyección de ingresos por cartera basada en cuotas vigentes y tasas de morosidad históricas.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PQRSummary,
                Name = "Resumen de PQR", Description = "Estadísticas y detalle de peticiones, quejas y reclamos recibidos en el período.",
                Category = ReportCategory.Operational, SourceModules = "PQR", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.CommonAreaUsage,
                Name = "Uso de Zonas Comunes", Description = "Reporte de reservas y uso de zonas comunes con estadísticas de ocupación.",
                Category = ReportCategory.Operational, SourceModules = "Reservations", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.MaintenanceSummary,
                Name = "Resumen de Mantenimiento", Description = "Resumen de órdenes de trabajo, incidentes y planes de mantenimiento ejecutados en el período.",
                Category = ReportCategory.Operational, SourceModules = "Maintenance", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.ActiveContracts,
                Name = "Contratos Vigentes", Description = "Listado de contratos de proveedores activos con sus valores, vigencias y estado.",
                Category = ReportCategory.Operational, SourceModules = "Suppliers,Contracts", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.CommunicationSummary,
                Name = "Resumen de Comunicaciones", Description = "Estadísticas de comunicaciones enviadas por tipo, destinatario y tasa de apertura.",
                Category = ReportCategory.Operational, SourceModules = "Communications", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AssemblyMinutes,
                Name = "Actas de Asamblea", Description = "Actas oficiales de las asambleas de propietarios realizadas.",
                Category = ReportCategory.Assembly, SourceModules = "Assembly", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AssemblyDecisions,
                Name = "Decisiones de Asamblea", Description = "Compendio de decisiones, votaciones y resultados de las asambleas de propietarios.",
                Category = ReportCategory.Assembly, SourceModules = "Assembly", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.CouncilHistory,
                Name = "Historial del Consejo", Description = "Historial de sesiones, decisiones y miembros del consejo de administración.",
                Category = ReportCategory.Assembly, SourceModules = "Assembly", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AssemblyQuorum,
                Name = "Quorum de Asamblea", Description = "Registro de quorum alcanzado en cada asamblea con detalle de unidades representadas.",
                Category = ReportCategory.Assembly, SourceModules = "Assembly,Units", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AnnualManagementReport,
                Name = "Informe Anual de Gestión", Description = "Informe consolidado de gestión del consejo de administración para la asamblea general anual de propietarios.",
                Category = ReportCategory.Annual, SourceModules = "All", AllowedRoles = "SuperAdmin,Admin,Council",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.OwnerRegistry,
                Name = "Padrón de Propietarios", Description = "Listado completo de propietarios con datos de contacto y unidades asignadas. Contiene datos personales.",
                Category = ReportCategory.Operational, SourceModules = "Residents,Units", AllowedRoles = "SuperAdmin,Admin,Accountant,Auditor",
                ContainsPersonalData = true, IsActive = true
            }
        };

        context.ReportTypes.AddRange(reports);
        await context.SaveChangesAsync();

        // Insertar plantilla PDF por defecto para cada tipo de reporte
        var templates = reports.Select(r => new PDFTemplate
        {
            TenantId = tenantId,
            ReportTypeCode = r.ReportTypeCode.ToString(),
            HeaderText = r.Name,
            FooterText = $"Página {{page}} de {{totalPages}}",
            SignatureName = "Administrador(a)",
            SignatureRole = "Administrador(a) del Conjunto",
            ConfidentialityNote = r.ContainsPersonalData
                ? "CONFIDENCIAL: Este informe contiene datos personales protegidos por la Ley 1581 de 2012. Su divulgación no autorizada está prohibida."
                : null,
            DisclaimerNote = r.Category == ReportCategory.Financial
                ? "Nota: Este informe financiero ha sido preparado con base en los registros presupuestales del conjunto. Los saldos están expresados en pesos colombianos (COP)."
                : null,
            PrimaryColor = "#059669",
            SecondaryColor = "#1e293b",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "system"
        }).ToList();

        context.PDFTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}
