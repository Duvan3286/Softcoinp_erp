using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class ClaimResolutionService
{
    private readonly ApplicationDbContext _context;

    public ClaimResolutionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ResolveClaimAsync(string tenantId, Guid pqrId, bool resolved, string resolutionNote, string userId)
    {
        var pqr = await _context.PqrRecords
            .Include(p => p.FollowUps)
            .FirstOrDefaultAsync(p => p.Id == pqrId && p.TenantId == tenantId);

        if (pqr == null)
        {
            throw new KeyNotFoundException("PQR no encontrada.");
        }

        if (pqr.PQRType != PQRType.Claim)
        {
            throw new InvalidOperationException("Solo los reclamos pueden ser resueltos mediante este proceso.");
        }

        if (!pqr.IsLinkedToCharge)
        {
            throw new InvalidOperationException("Esta PQR no está vinculada a ningún cobro.");
        }

        if (pqr.ClaimResolved.HasValue)
        {
            throw new InvalidOperationException("Este reclamo ya ha sido resuelto.");
        }

        pqr.ClaimResolved = resolved;
        pqr.ClaimResolutionNote = resolutionNote;
        pqr.UpdatedAt = DateTime.UtcNow;

        string sourceLabel;
        decimal chargeAmount = 0m;
        string chargeType = string.Empty;
        Guid? chargeId = null;

        if (pqr.UnitFeeId.HasValue)
        {
            chargeType = "UnitFee";
            chargeId = pqr.UnitFeeId.Value;

            var unitFee = await _context.UnitFees
                .FirstOrDefaultAsync(f => f.Id == chargeId.Value && f.TenantId == tenantId);

            if (unitFee == null)
            {
                throw new KeyNotFoundException("La cuota ordinaria vinculada no fue encontrada.");
            }

            chargeAmount = unitFee.BalanceAmount;
            sourceLabel = $"Cuota ordinaria {unitFee.DueDate:yyyy-MM} por {unitFee.FeeValue:C2}";
        }
        else if (pqr.ExtraordinaryFeeDistributionId.HasValue)
        {
            chargeType = "ExtraordinaryFee";
            chargeId = pqr.ExtraordinaryFeeDistributionId.Value;

            var distribution = await _context.ExtraordinaryFeeDistributions
                .FirstOrDefaultAsync(d => d.Id == chargeId.Value && d.TenantId == tenantId);

            if (distribution == null)
            {
                throw new KeyNotFoundException("La distribución de cuota extraordinaria vinculada no fue encontrada.");
            }

            chargeAmount = distribution.BalanceAmount;
            sourceLabel = $"Cuota extraordinaria #{distribution.InstallmentNumber} por {distribution.Amount:C2}";
        }
        else if (pqr.IndividualChargeId.HasValue)
        {
            chargeType = "IndividualCharge";
            chargeId = pqr.IndividualChargeId.Value;

            var charge = await _context.IndividualCharges
                .FirstOrDefaultAsync(c => c.Id == chargeId.Value && c.TenantId == tenantId);

            if (charge == null)
            {
                throw new KeyNotFoundException("El cobro individual vinculado no fue encontrado.");
            }

            chargeAmount = charge.BalanceAmount;
            sourceLabel = $"Cobro individual: {charge.Concept} por {charge.Amount:C2}";
        }
        else
        {
            throw new InvalidOperationException("El reclamo está marcado como vinculado a cobro pero no tiene una referencia de cobro específica.");
        }

        if (resolved && chargeAmount > 0m)
        {
            await ApplyCreditNoteAsync(tenantId, pqr, chargeType, chargeId.Value, chargeAmount, sourceLabel, userId);
        }

        var followUp = new PqrFollowUp
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            PreviousStatus = pqr.Status,
            NewStatus = pqr.Status,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ChangedByUserName = "Administrador",
            Justification = resolved
                ? $"Reclamo declarado PROCEDENTE. {resolutionNote}"
                : $"Reclamo declarado IMPROCEDENTE. {resolutionNote}",
            IsAutomatic = false
        };

        _context.PqrFollowUps.Add(followUp);
        await _context.SaveChangesAsync();
    }

    private async Task ApplyCreditNoteAsync(
        string tenantId, PqrRecord pqr, string chargeType,
        Guid chargeId, decimal chargeAmount, string sourceLabel, string userId)
    {
        var description = $"Nota de crédito por reclamo {pqr.RadicadoNumber}. {sourceLabel}. {pqr.ClaimResolutionNote}";

        switch (chargeType)
        {
            case "UnitFee":
                var unitFee = await _context.UnitFees.FindAsync(chargeId);
                if (unitFee != null)
                {
                    unitFee.BalanceAmount = 0m;
                    unitFee.PaidAmount = unitFee.FeeValue;
                    unitFee.Status = FeeStatus.FullyPaid;
                }
                break;

            case "ExtraordinaryFee":
                var distribution = await _context.ExtraordinaryFeeDistributions.FindAsync(chargeId);
                if (distribution != null)
                {
                    distribution.BalanceAmount = 0m;
                    distribution.PaidAmount = distribution.Amount;
                    distribution.Status = FeeStatus.FullyPaid;
                }
                break;

            case "IndividualCharge":
                var charge = await _context.IndividualCharges.FindAsync(chargeId);
                if (charge != null)
                {
                    charge.BalanceAmount = 0m;
                    charge.PaidAmount = charge.Amount;
                    charge.Status = IndividualChargeStatus.Paid;
                }
                break;
        }

        pqr.CreditNoteGenerated = true;
    }
}
