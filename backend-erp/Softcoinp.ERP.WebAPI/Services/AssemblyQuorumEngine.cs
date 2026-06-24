using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class AssemblyQuorumEngine
{
    private readonly ApplicationDbContext _context;

    public AssemblyQuorumEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetTotalCoefficientsAsync(string tenantId)
    {
        return await _context.Units
            .Where(u => u.TenantId == tenantId &&
                       (u.Status == Domain.Enums.UnitStatus.ActiveOccupied ||
                        u.Status == Domain.Enums.UnitStatus.ActiveUnoccupied))
            .SumAsync(u => u.CoproprietyCoefficient);
    }

    public async Task<QuorumStatus> CalculateQuorumAsync(Guid assemblyId, string tenantId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        var totalCoefficients = await GetTotalCoefficientsAsync(tenantId);

        var attendances = await _context.AssemblyAttendances
            .Where(a => a.AssemblyId == assemblyId && a.TenantId == tenantId)
            .ToListAsync();

        var presentAttendances = attendances
            .Where(a => a.Status == Domain.Enums.AttendanceStatus.Present ||
                       a.Status == Domain.Enums.AttendanceStatus.Represented)
            .ToList();

        var presentCoefficients = presentAttendances.Sum(a => a.Coefficient);

        var firstCallThreshold = totalCoefficients * 0.5m;
        var secondCallThreshold = 0m;

        var firstCallMet = presentCoefficients > firstCallThreshold;
        var secondCallMet = assembly.ConvocationNumber >= 2;

        var ownersWithArrears = attendances.Count(a => a.HasDuesArrears);
        var ownersWithRestrictedVoting = attendances.Count(a => a.VotingRightRestricted);

        return new QuorumStatus
        {
            TotalCoefficients = totalCoefficients,
            PresentCoefficients = presentCoefficients,
            QuorumThresholdFirstCall = firstCallThreshold,
            QuorumThresholdSecondCall = secondCallThreshold,
            FirstCallQuorumMet = firstCallMet,
            SecondCallQuorumMet = secondCallMet,
            PercentagePresent = totalCoefficients > 0
                ? Math.Round(presentCoefficients / totalCoefficients * 100, 2)
                : 0,
            TotalOwners = attendances.Count,
            PresentOwners = presentAttendances.Count,
            AbsentOwners = attendances.Count(a => a.Status == Domain.Enums.AttendanceStatus.Absent),
            OwnersWithArrears = ownersWithArrears,
            OwnersWithRestrictedVoting = ownersWithRestrictedVoting
        };
    }

    public async Task<List<AssemblyAttendance>> GetEligibleVotersAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyAttendances
            .Where(a => a.AssemblyId == assemblyId &&
                       a.TenantId == tenantId &&
                       (a.Status == Domain.Enums.AttendanceStatus.Present ||
                        a.Status == Domain.Enums.AttendanceStatus.Represented))
            .ToListAsync();
    }

    public async Task<decimal> GetEligibleVotingCoefficientsAsync(Guid assemblyId, string tenantId)
    {
        var voters = await GetEligibleVotersAsync(assemblyId, tenantId);

        return voters
            .Where(a => !a.VotingRightRestricted)
            .Sum(a => a.Coefficient);
    }

    public async Task<List<UnitWithOwnerInfo>> GetAllUnitsWithOwnersAsync(string tenantId)
    {
        return await _context.Units
            .Where(u => u.TenantId == tenantId &&
                       (u.Status == Domain.Enums.UnitStatus.ActiveOccupied ||
                        u.Status == Domain.Enums.UnitStatus.ActiveUnoccupied))
            .Select(u => new UnitWithOwnerInfo
            {
                UnitId = u.Id,
                UnitIdentifier = u.Identifier,
                Coefficient = u.CoproprietyCoefficient,
                OwnerId = u.UnitOwners
                    .Where(uo => uo.IsActive)
                    .Select(uo => uo.OwnerId)
                    .FirstOrDefault(),
                OwnerName = u.UnitOwners
                    .Where(uo => uo.IsActive)
                    .Select(uo => uo.Owner != null ? uo.Owner.FullNameOrCompanyName : "")
                    .FirstOrDefault(),
                OwnerEmail = u.UnitOwners
                    .Where(uo => uo.IsActive)
                    .Select(uo => uo.Owner != null ? uo.Owner.Email : "")
                    .FirstOrDefault(),
                OwnerPhone = u.UnitOwners
                    .Where(uo => uo.IsActive)
                    .Select(uo => uo.Owner != null ? uo.Owner.MainPhone : "")
                    .FirstOrDefault()
            })
            .ToListAsync();
    }
}

public class QuorumStatus
{
    public decimal TotalCoefficients { get; set; }
    public decimal PresentCoefficients { get; set; }
    public decimal QuorumThresholdFirstCall { get; set; }
    public decimal QuorumThresholdSecondCall { get; set; }
    public bool FirstCallQuorumMet { get; set; }
    public bool SecondCallQuorumMet { get; set; }
    public decimal PercentagePresent { get; set; }
    public int TotalOwners { get; set; }
    public int PresentOwners { get; set; }
    public int AbsentOwners { get; set; }
    public int OwnersWithArrears { get; set; }
    public int OwnersWithRestrictedVoting { get; set; }
}

public class UnitWithOwnerInfo
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal Coefficient { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public string? OwnerPhone { get; set; }
}
