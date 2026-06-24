using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class AssemblyVotingEngine
{
    private readonly ApplicationDbContext _context;

    public AssemblyVotingEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VotingResult> CalculateVotingResultAsync(
        Guid assemblyId, Guid agendaItemId, string tenantId,
        decimal votesInFavor, decimal votesAgainst, decimal abstentions)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        var agendaItem = await _context.AssemblyAgendaItems
            .FirstOrDefaultAsync(ai => ai.Id == agendaItemId && ai.TenantId == tenantId);

        if (agendaItem == null)
            throw new InvalidOperationException("Agenda item not found");

        var totalCoefficients = await GetTotalCoefficientsAsync(tenantId);
        var eligibleCoefficients = await GetEligibleVotingCoefficientsAsync(assemblyId, tenantId);

        var totalVotesCast = votesInFavor + votesAgainst + abstentions;
        var validVotes = votesInFavor + votesAgainst;

        bool isApproved = false;
        decimal approvalThreshold = 0;
        string rejectionReason = string.Empty;

        switch (agendaItem.MajorityRequired)
        {
            case MajorityType.Simple:
                approvalThreshold = eligibleCoefficients * 0.5m;
                isApproved = validVotes > 0 && votesInFavor > approvalThreshold;
                if (!isApproved)
                    rejectionReason = $"Se requieren más de {Math.Round(approvalThreshold, 4)} coeficientes a favor (mayoría simple de coeficientes presentes con derecho a voto). Se obtuvieron {votesInFavor}.";
                break;

            case MajorityType.Qualified:
                approvalThreshold = totalCoefficients * 0.7m;
                isApproved = votesInFavor >= approvalThreshold;
                if (!isApproved)
                    rejectionReason = $"Se requieren al menos {Math.Round(approvalThreshold, 4)} coeficientes a favor (70% del total del conjunto). Se obtuvieron {votesInFavor}.";
                break;

            case MajorityType.Unanimity:
                approvalThreshold = totalCoefficients;
                isApproved = votesInFavor >= totalCoefficients;
                if (!isApproved)
                    rejectionReason = $"Se requiere la totalidad de coeficientes a favor ({totalCoefficients}). Se obtuvieron {votesInFavor}.";
                break;
        }

        return new VotingResult
        {
            IsApproved = isApproved,
            ApprovalThreshold = approvalThreshold,
            TotalCoefficients = totalCoefficients,
            EligibleCoefficients = eligibleCoefficients,
            VotesInFavor = votesInFavor,
            VotesAgainst = votesAgainst,
            Abstentions = abstentions,
            TotalVotesCast = totalVotesCast,
            ValidVotes = validVotes,
            RejectionReason = rejectionReason,
            MajorityRequired = agendaItem.MajorityRequired.ToString()
        };
    }

    public async Task<List<AssemblyAttendance>> GetVotersWithArrearsAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyAttendances
            .Where(a => a.AssemblyId == assemblyId &&
                       a.TenantId == tenantId &&
                       a.HasDuesArrears &&
                       !a.VotingRightRestricted)
            .ToListAsync();
    }

    public async Task RestrictVotingForArrearsAsync(Guid assemblyId, string tenantId, string adminUserId)
    {
        var votersWithArrears = await GetVotersWithArrearsAsync(assemblyId, tenantId);

        foreach (var attendance in votersWithArrears)
        {
            attendance.VotingRightRestricted = true;
            attendance.VotingRestrictionReason = "Propietario con deuda según artículo 40 de la Ley 675 de 2001";
            attendance.RegisteredByUserId = adminUserId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task LiftVotingRestrictionAsync(
        Guid attendanceId, string tenantId, string reason, string adminUserId)
    {
        var attendance = await _context.AssemblyAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId && a.TenantId == tenantId);

        if (attendance == null)
            throw new InvalidOperationException("Attendance record not found");

        attendance.VotingRightRestricted = false;
        attendance.VotingRestrictionLiftedByUserId = adminUserId;
        attendance.VotingRestrictionLiftedReason = reason;
        attendance.VotingRestrictionLiftedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<decimal> GetTotalCoefficientsAsync(string tenantId)
    {
        return await _context.Units
            .Where(u => u.TenantId == tenantId &&
                       (u.Status == UnitStatus.ActiveOccupied ||
                        u.Status == UnitStatus.ActiveUnoccupied))
            .SumAsync(u => u.CoproprietyCoefficient);
    }

    private async Task<decimal> GetEligibleVotingCoefficientsAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyAttendances
            .Where(a => a.AssemblyId == assemblyId &&
                       a.TenantId == tenantId &&
                       (a.Status == AttendanceStatus.Present ||
                        a.Status == AttendanceStatus.Represented) &&
                       !a.VotingRightRestricted)
            .SumAsync(a => a.Coefficient);
    }
}

public class VotingResult
{
    public bool IsApproved { get; set; }
    public decimal ApprovalThreshold { get; set; }
    public decimal TotalCoefficients { get; set; }
    public decimal EligibleCoefficients { get; set; }
    public decimal VotesInFavor { get; set; }
    public decimal VotesAgainst { get; set; }
    public decimal Abstentions { get; set; }
    public decimal TotalVotesCast { get; set; }
    public decimal ValidVotes { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string MajorityRequired { get; set; } = string.Empty;
}
