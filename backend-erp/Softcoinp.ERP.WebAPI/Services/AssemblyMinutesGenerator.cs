using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class AssemblyMinutesGenerator
{
    private readonly ApplicationDbContext _context;

    public AssemblyMinutesGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateMinutesTextAsync(Guid assemblyId, string tenantId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        var attendances = await _context.AssemblyAttendances
            .Include(a => a.Unit)
            .Include(a => a.Owner)
            .Include(a => a.RepresentativeOwner)
            .Where(a => a.AssemblyId == assemblyId && a.TenantId == tenantId)
            .OrderBy(a => a.Unit != null ? a.Unit.Identifier : "")
            .ToListAsync();

        var agendaItems = await _context.AssemblyAgendaItems
            .Where(ai => ai.AssemblyId == assemblyId && ai.TenantId == tenantId)
            .OrderBy(ai => ai.SequenceNumber)
            .ToListAsync();

        var constancies = await _context.AssemblyConstancies
            .Where(c => c.AssemblyId == assemblyId && c.TenantId == tenantId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var totalCoefficients = await _context.Units
            .Where(u => u.TenantId == tenantId &&
                       (u.Status == UnitStatus.ActiveOccupied ||
                        u.Status == UnitStatus.ActiveUnoccupied))
            .SumAsync(u => u.CoproprietyCoefficient);

        var presentAttendances = attendances
            .Where(a => a.Status == AttendanceStatus.Present ||
                       a.Status == AttendanceStatus.Represented)
            .ToList();

        var presentCoefficients = presentAttendances.Sum(a => a.Coefficient);

        var sb = new StringBuilder();

        sb.AppendLine("ACTA DE ASAMBLEA GENERAL DE COPROPIETARIOS");
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine();

        sb.AppendLine($"Tipo de Asamblea: {(assembly.Type == AssemblyType.Ordinary ? "ORDINARIA" : "EXTRAORDINARIA")}");
        sb.AppendLine($"Fecha: {assembly.ScheduledDate:dd/MM/yyyy}");
        sb.AppendLine($"Hora: {assembly.ScheduledTime}");
        sb.AppendLine($"Lugar: {assembly.Location}");
        sb.AppendLine();

        sb.AppendLine("I. CONVOCATORIA");
        sb.AppendLine("─────────────────");
        sb.AppendLine($"Convocatoria N.° {assembly.ConvocationNumber}");
        sb.AppendLine($"Fecha de envío: {(assembly.ConvocationSentAt ?? "No registrada")}");
        sb.AppendLine($"Cumplimiento del plazo legal: {(assembly.ConvocationDeadlineMet ? "SÍ" : "NO")}");
        sb.AppendLine();

        sb.AppendLine("II. VERIFICACIÓN DEL QUÓRUM");
        sb.AppendLine("────────────────────────────");
        sb.AppendLine($"Coeficientes totales del conjunto: {Math.Round(totalCoefficients, 4)}");
        sb.AppendLine($"Coeficientes presentes: {Math.Round(presentCoefficients, 4)}");
        sb.AppendLine($"Porcentaje de asistencia: {Math.Round(presentCoefficients / totalCoefficients * 100, 2)}%");

        if (assembly.ConvocationNumber >= 2)
        {
            sb.AppendLine($"Segunda convocatoria: SÍ (a partir de las {assembly.SecondConvocationTime ?? "N/A"})");
        }

        sb.AppendLine($"Quórum alcanzado: {(presentCoefficients > totalCoefficients * 0.5m ? "SÍ" : "NO")}");
        sb.AppendLine();

        sb.AppendLine("III. LISTA DE ASISTENTES");
        sb.AppendLine("────────────────────────");
        sb.AppendLine($"{"Propietario",-30} {"Unidad",-10} {"Coef.",-10} {"Condición",-15} {"Representante",-25}");
        sb.AppendLine(new string('─', 90));

        foreach (var a in presentAttendances)
        {
            var condition = a.HasDuesArrears ? "En mora" : "Al día";
            if (a.VotingRightRestricted) condition += " (sin voto)";
            var representative = a.AttendsPersonally ? "Personal" : (a.RepresentativeName ?? "N/A");
            var unitId = a.Unit != null ? a.Unit.Identifier : "N/A";
            var ownerName = a.Owner != null ? a.Owner.FullNameOrCompanyName : "N/A";
            sb.AppendLine($"{ownerName,-30} {unitId,-10} {Math.Round(a.Coefficient, 4),-10} {condition,-15} {representative,-25}");
        }

        sb.AppendLine();

        sb.AppendLine("IV. ORDEN DEL DÍA Y DECISIONES");
        sb.AppendLine("───────────────────────────────");

        foreach (var item in agendaItems)
        {
            sb.AppendLine();
            sb.AppendLine($"Punto {item.SequenceNumber}: {item.Title.ToUpper()}");

            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine($"  Descripción: {item.Description}");
            }

            if (!string.IsNullOrEmpty(item.PresenterName))
            {
                sb.AppendLine($"  Ponente: {item.PresenterName}");
            }

            if (item.IsInformationOnly)
            {
                sb.AppendLine("  Tipo: INFORMATIVO (sin votación)");
            }
            else
            {
                sb.AppendLine($"  Tipo de mayoría requerida: {GetMajorityLabel(item.MajorityRequired)}");
                sb.AppendLine($"  Modalidad de votación: {(item.VotingMode == VotingMode.Public ? "PÚBLICA (a mano alzada)" : "SECRETA (por tarjeta)")}");

                if (item.VoteRegistered)
                {
                    sb.AppendLine($"  Resultado de la votación:");
                    sb.AppendLine($"    A favor:     {Math.Round(item.VotesInFavorCoefficients, 4)} coeficientes ({item.VotesInFavorCount} votos)");
                    sb.AppendLine($"    En contra:   {Math.Round(item.VotesAgainstCoefficients, 4)} coeficientes ({item.VotesAgainstCount} votos)");
                    sb.AppendLine($"    Abstenciones: {Math.Round(item.AbstentionCoefficients, 4)} coeficientes ({item.AbstentionCount} votos)");
                    sb.AppendLine($"  DECISIÓN: {(item.IsApproved == true ? "APROBADA" : "NO APROBADA")}");

                    if (!item.IsApproved == true && !string.IsNullOrEmpty(item.RejectionReason))
                    {
                        sb.AppendLine($"  Motivo de rechazo: {item.RejectionReason}");
                    }
                }
                else
                {
                    sb.AppendLine("  Votación: No registrada");
                }
            }

            if (!string.IsNullOrEmpty(item.Observations))
            {
                sb.AppendLine($"  Observaciones: {item.Observations}");
            }
        }

        sb.AppendLine();

        sb.AppendLine("V. CONSTANCIAS");
        sb.AppendLine("──────────────");

        if (constancies.Any())
        {
            foreach (var c in constancies)
            {
                sb.AppendLine($"- {c.OwnerName}: {c.Text}");
                if (c.AgendaItemId.HasValue)
                {
                    var agendaTitle = agendaItems.FirstOrDefault(ai => ai.Id == c.AgendaItemId)?.Title;
                    if (agendaTitle != null)
                    {
                        sb.AppendLine($"  (Referente al punto: {agendaTitle})");
                    }
                }
            }
        }
        else
        {
            sb.AppendLine("No se registraron constancias.");
        }

        sb.AppendLine();

        sb.AppendLine("VI. CIERRE DE LA SESIÓN");
        sb.AppendLine("──────────────────────");
        sb.AppendLine($"Hora de inicio: {assembly.SessionStartTime?.ToString("HH:mm") ?? "No registrada"}");
        sb.AppendLine($"Hora de cierre: {assembly.SessionEndTime?.ToString("HH:mm") ?? "No registrada"}");
        sb.AppendLine();

        sb.AppendLine("VII. FIRMAS");
        sb.AppendLine("────────────");
        sb.AppendLine();
        sb.AppendLine("_________________________                    _________________________");
        sb.AppendLine($"Presidente de la Asamblea                    Secretario de la Asamblea");
        sb.AppendLine($"{assembly.PresidentName ?? "_________________________"}                    {assembly.SecretaryName ?? "_________________________"}");
        sb.AppendLine();
        sb.AppendLine($"Acta generada electrónicamente el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC");
        sb.AppendLine("Este documento será válido una vez firmado por el Presidente y Secretario de la Asamblea.");

        return sb.ToString();
    }

    public async Task<AssemblyMinutes> CreateMinutesAsync(
        Guid assemblyId, string tenantId, string userId,
        string? presidentName = null, string? secretaryName = null,
        string? commissionMemberNames = null)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.Closed)
            throw new InvalidOperationException("Assembly must be closed before generating minutes");

        var existingMinutes = await _context.AssemblyMinutes
            .FirstOrDefaultAsync(m => m.AssemblyId == assemblyId && m.TenantId == tenantId);

        if (existingMinutes != null)
            throw new InvalidOperationException("Minutes already exist for this assembly");

        var fullText = await GenerateMinutesTextAsync(assemblyId, tenantId);

        var commissionDeadline = DateTime.UtcNow.AddDays(5);

        var minutes = new AssemblyMinutes
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            Status = MinutesStatus.Draft,
            PresidentName = presidentName ?? assembly.PresidentName,
            SecretaryName = secretaryName ?? assembly.SecretaryName,
            FullText = fullText,
            GeneratedAt = DateTime.UtcNow,
            GeneratedByUserId = userId,
            CommissionMemberNames = commissionMemberNames,
            CommissionReviewDeadline = commissionDeadline,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.AssemblyMinutes.Add(minutes);
        await _context.SaveChangesAsync();

        return minutes;
    }

    private string GetMajorityLabel(MajorityType majority)
    {
        switch (majority)
        {
            case MajorityType.Simple:
                return "Mayoría simple (>50% de coeficientes presentes con derecho a voto)";
            case MajorityType.Qualified:
                return "Mayoría calificada (≥70% de coeficientes totales del conjunto)";
            case MajorityType.Unanimity:
                return "Unanimidad (100% de coeficientes totales del conjunto)";
            default:
                return majority.ToString();
        }
    }
}
