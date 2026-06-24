using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class AssemblyDecisionPropagationService
{
    private readonly ApplicationDbContext _context;

    public AssemblyDecisionPropagationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssemblyDecisionPropagation> CreatePropagationAsync(
        Guid assemblyId, Guid agendaItemId, string tenantId,
        DecisionPropagationTarget targetModule, string description)
    {
        var propagation = new AssemblyDecisionPropagation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            AgendaItemId = agendaItemId,
            TargetModule = targetModule,
            Status = DecisionPropagationStatus.Pending,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.AssemblyDecisionPropagations.Add(propagation);
        await _context.SaveChangesAsync();

        return propagation;
    }

    public async Task MarkAsPropagatedAsync(Guid propagationId, string tenantId, string targetEntityId, string targetEntityType)
    {
        var propagation = await _context.AssemblyDecisionPropagations
            .FirstOrDefaultAsync(p => p.Id == propagationId && p.TenantId == tenantId);

        if (propagation == null)
            throw new InvalidOperationException("Propagation record not found");

        propagation.Status = DecisionPropagationStatus.Propagated;
        propagation.TargetEntityId = targetEntityId;
        propagation.TargetEntityType = targetEntityType;
        propagation.PropagatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsFailedAsync(Guid propagationId, string tenantId, string errorMessage)
    {
        var propagation = await _context.AssemblyDecisionPropagations
            .FirstOrDefaultAsync(p => p.Id == propagationId && p.TenantId == tenantId);

        if (propagation == null)
            throw new InvalidOperationException("Propagation record not found");

        propagation.Status = DecisionPropagationStatus.Failed;
        propagation.ErrorMessage = errorMessage;
        propagation.RetryCount += 1;

        await _context.SaveChangesAsync();
    }

    public async Task<List<AssemblyDecisionPropagation>> GetPendingPropagationsAsync(string tenantId)
    {
        return await _context.AssemblyDecisionPropagations
            .Where(p => p.TenantId == tenantId && p.Status == DecisionPropagationStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AssemblyDecisionPropagation>> GetPropagationsByAssemblyAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyDecisionPropagations
            .Where(p => p.AssemblyId == assemblyId && p.TenantId == tenantId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> RetryPendingPropagationsAsync(string tenantId)
    {
        var pending = await _context.AssemblyDecisionPropagations
            .Where(p => p.TenantId == tenantId &&
                       p.Status == DecisionPropagationStatus.Failed &&
                       p.RetryCount < 3)
            .ToListAsync();

        var retriedCount = 0;

        foreach (var propagation in pending)
        {
            propagation.Status = DecisionPropagationStatus.Pending;
            propagation.ErrorMessage = null;
            retriedCount++;
        }

        if (retriedCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return retriedCount;
    }
}
