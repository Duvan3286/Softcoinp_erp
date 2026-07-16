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
    public static async Task SeedUsersAsync(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration? configuration = null)
    {
        var roles = Enum.GetNames<AppRole>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

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
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

            var currentRoles = await userManager.GetRolesAsync(adminUser);
            if (!currentRoles.Contains(nameof(AppRole.SuperAdmin)))
            {
                await userManager.RemoveFromRolesAsync(adminUser, currentRoles);
                await userManager.AddToRoleAsync(adminUser, nameof(AppRole.SuperAdmin));
            }
        }
    }

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
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PortfolioReport,
                Name = "Cartera General", Description = "Resumen completo de la cartera del conjunto con saldos por unidad y antiguedad.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio,Units",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.CollectionReport,
                Name = "Recaudo del Periodo", Description = "Resumen de pagos recibidos en el periodo, incluyendo forma de pago y unidad.",
                Category = ReportCategory.Portfolio, SourceModules = "Billing,Portfolio",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.ExpenseReport,
                Name = "Gastos del Periodo", Description = "Detalle de todos los gastos registrados en el periodo agrupados por rubro presupuestal.",
                Category = ReportCategory.Financial, SourceModules = "Expenses,Budgets",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.BudgetExecution,
                Name = "Ejecucion Presupuestal", Description = "Comparacion entre el presupuesto aprobado y los gastos/ingresos reales ejecutados en el periodo.",
                Category = ReportCategory.Financial, SourceModules = "Budgets",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.ActiveContracts,
                Name = "Contratos Vigentes", Description = "Listado de contratos de proveedores activos con sus valores, vigencias y estado.",
                Category = ReportCategory.Operational, SourceModules = "Suppliers,Contracts",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.PQRReport,
                Name = "Informe de PQR", Description = "Estadisticas y detalle de peticiones, quejas y reclamos recibidos en el periodo.",
                Category = ReportCategory.Operational, SourceModules = "PQR",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.MaintenanceReport,
                Name = "Informe de Mantenimiento", Description = "Resumen de ordenes de trabajo, incidentes y planes de mantenimiento ejecutados en el periodo.",
                Category = ReportCategory.Operational, SourceModules = "Maintenance",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AssemblyReport,
                Name = "Informe de Asamblea", Description = "Actas oficiales de las asambleas de propietarios realizadas con decisiones y quorum.",
                Category = ReportCategory.Assembly, SourceModules = "Assembly",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AnnualManagementReport,
                Name = "Informe Anual de Gestion", Description = "Informe consolidado de gestion para la asamblea general anual de propietarios.",
                Category = ReportCategory.Annual, SourceModules = "All",
                ContainsPersonalData = false, IsActive = true
            },
            new()
            {
                TenantId = tenantId, ReportTypeCode = ReportTypeEnum.AccountantExport,
                Name = "Exportacion para el Contador", Description = "Exportacion en Excel con hojas separadas de ingresos y egresos del periodo.",
                Category = ReportCategory.Financial, SourceModules = "Payments,ProviderPayments",
                ContainsPersonalData = true, IsActive = true
            }
        };

        context.ReportTypes.AddRange(reports);
        await context.SaveChangesAsync();

        var globalTemplate = new PDFTemplate
        {
            TenantId = tenantId,
            IsGlobal = true,
            HeaderText = "Informe de Gestion",
            FooterText = "Pagina {{page}} de {{totalPages}}",
            SignatureName = "Administrador(a)",
            SignatureRole = "Administrador(a) del Conjunto",
            ConfidentialityNote = "CONFIDENCIAL: Este informe contiene informacion de gestion del conjunto. Su divulgacion no autorizada esta prohibida.",
            DisclaimerNote = "Nota: Este informe ha sido preparado con base en los registros del conjunto. Las cifras estan expresadas en pesos colombianos (COP).",
            PrimaryColor = "#059669",
            SecondaryColor = "#1e293b",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "system"
        };

        context.PDFTemplates.Add(globalTemplate);
        await context.SaveChangesAsync();
    }

    public static async Task SeedNotificationTemplatesAsync(ApplicationDbContext context, string tenantId)
    {
        var hasTemplates = await context.NotificationTemplates.AnyAsync(t => t.TenantId == tenantId);
        if (hasTemplates)
        {
            return;
        }

        var templates = new List<NotificationTemplate>
        {
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.PaymentConfirmed,
                Name = "Confirmacion de Pago", ForRecipientType = RecipientType.Both,
                EmailSubject = "Confirmacion de pago recibido - Unidad {UnitIdentifier}",
                EmailBody = "Hola {ResidentName}, confirmamos la recepcion de su pago por valor de {Amount} para la unidad {UnitIdentifier} con fecha {PaymentDate}. Gracias por su puntualidad.",
                SmsBody = "Confirmamos su pago de {Amount} para la unidad {UnitIdentifier}. Gracias.",
                DynamicVariables = "ResidentName,Amount,UnitIdentifier,PaymentDate",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.PQRReceived,
                Name = "PQR Radicada", ForRecipientType = RecipientType.Both,
                EmailSubject = "PQR Radicada: {RadicadoNumber}",
                EmailBody = "Hola {ResidentName}, su solicitud fue radicada con el numero {RadicadoNumber}. Fecha limite de respuesta: {Deadline}. La administracion dara respuesta dentro del plazo establecido.",
                SmsBody = "Su PQR {RadicadoNumber} fue radicada. Plazo de respuesta: {Deadline}.",
                DynamicVariables = "ResidentName,RadicadoNumber,Deadline",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.PQRResponseAvailable,
                Name = "Respuesta a PQR Disponible", ForRecipientType = RecipientType.Both,
                EmailSubject = "Respuesta a su PQR {RadicadoNumber}",
                EmailBody = "Hola {ResidentName}, la administracion ha respondido su solicitud {RadicadoNumber}. Ingrese al portal para ver el detalle de la respuesta.",
                SmsBody = "Su PQR {RadicadoNumber} ya tiene respuesta. Ingrese al portal para verla.",
                DynamicVariables = "ResidentName,RadicadoNumber",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.ReservationApproved,
                Name = "Reserva Aprobada", ForRecipientType = RecipientType.Both,
                EmailSubject = "Reserva aprobada: {SpaceName}",
                EmailBody = "Hola {ResidentName}, su reserva de {SpaceName} para el {ReservationDate} a las {ReservationTime} ha sido aprobada.",
                SmsBody = "Su reserva de {SpaceName} el {ReservationDate} fue aprobada.",
                DynamicVariables = "ResidentName,SpaceName,ReservationDate,ReservationTime",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.ReservationReminder24h,
                Name = "Recordatorio de Reserva (24h)", ForRecipientType = RecipientType.Both,
                EmailSubject = "Recordatorio: su reserva es manana",
                EmailBody = "Hola {ResidentName}, le recordamos que su reserva de {SpaceName} es el {ReservationDate} a las {ReservationTime}.",
                SmsBody = "Recordatorio: reserva de {SpaceName} manana {ReservationTime}.",
                DynamicVariables = "ResidentName,SpaceName,ReservationDate,ReservationTime",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.ReservationReminder2h,
                Name = "Recordatorio de Reserva (2h)", ForRecipientType = RecipientType.Both,
                EmailSubject = "Recordatorio: su reserva es en 2 horas",
                EmailBody = "Hola {ResidentName}, le recordamos que su reserva de {SpaceName} es hoy {ReservationDate} a las {ReservationTime}, en 2 horas.",
                SmsBody = "Recordatorio: reserva de {SpaceName} en 2 horas, {ReservationTime}.",
                DynamicVariables = "ResidentName,SpaceName,ReservationDate,ReservationTime",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.AssemblyConvocation,
                Name = "Convocatoria de Asamblea", ForRecipientType = RecipientType.Owner,
                EmailSubject = "Convocatoria a Asamblea de Propietarios",
                EmailBody = "Se le convoca a la asamblea de propietarios a realizarse el {AssemblyDate} a las {AssemblyTime} en {Location}. Su asistencia es importante.",
                SmsBody = "Convocatoria a asamblea el {AssemblyDate} {AssemblyTime} en {Location}.",
                DynamicVariables = "AssemblyDate,AssemblyTime,Location",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.AssemblyMinutesPublished,
                Name = "Acta de Asamblea Publicada", ForRecipientType = RecipientType.Owner,
                EmailSubject = "Acta de asamblea disponible",
                EmailBody = "El acta de la asamblea del {AssemblyDate} (Acta N. {ActNumber}) ya esta disponible para consulta en el portal.",
                SmsBody = "El acta de asamblea N. {ActNumber} ya esta disponible.",
                DynamicVariables = "AssemblyDate,ActNumber",
                CreatedByUserId = "system"
            },
            new()
            {
                TenantId = tenantId, EventType = NotificationEventType.WorkOrderResolved,
                Name = "Orden de Mantenimiento Resuelta", ForRecipientType = RecipientType.Both,
                EmailSubject = "Su solicitud de mantenimiento fue atendida",
                EmailBody = "Hola {ResidentName}, la orden de mantenimiento relacionada con su solicitud {RadicadoNumber} fue completada.",
                SmsBody = "Su solicitud de mantenimiento {RadicadoNumber} fue atendida.",
                DynamicVariables = "ResidentName,RadicadoNumber",
                CreatedByUserId = "system"
            }
        };

        context.NotificationTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}
