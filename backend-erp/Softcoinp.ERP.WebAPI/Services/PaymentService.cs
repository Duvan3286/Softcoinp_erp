using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private readonly IndicatorCacheService _indicatorCache;
    private readonly NotificationEngine _notificationEngine;

    public PaymentService(
        ApplicationDbContext context,
        ILogger<PaymentService> logger,
        IndicatorCacheService indicatorCache,
        NotificationEngine notificationEngine)
    {
        _context = context;
        _logger = logger;
        _indicatorCache = indicatorCache;
        _notificationEngine = notificationEngine;
    }

    public async Task<UnitDebtSummaryDto> GetUnitDebtSummaryAsync(string tenantId, Guid unitId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
        {
            throw new KeyNotFoundException("No se encontró la unidad.");
        }

        var now = DateTime.UtcNow;
        var items = new List<DebtItemDto>();

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.UnitId == unitId && uf.BalanceAmount > 0)
            .OrderBy(uf => uf.DueDate)
            .ToListAsync();

        foreach (var fee in overdueFees)
        {
            items.Add(new DebtItemDto
            {
                SourceType = "UnitFee",
                SourceId = fee.Id,
                Description = "Cuota ordinaria " + fee.DueDate.ToString("yyyy-MM"),
                DueDate = fee.DueDate,
                Amount = fee.FeeValue,
                Balance = fee.BalanceAmount,
                IsOverdue = fee.DueDate < now
            });
        }

        var overdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId && ed.UnitId == unitId && ed.BalanceAmount > 0)
            .OrderBy(ed => ed.DueDate)
            .ToListAsync();

        foreach (var dist in overdueExtraordinary)
        {
            items.Add(new DebtItemDto
            {
                SourceType = "ExtraordinaryFee",
                SourceId = dist.Id,
                Description = "Cuota extraordinaria #" + dist.InstallmentNumber,
                DueDate = dist.DueDate,
                Amount = dist.Amount,
                Balance = dist.BalanceAmount,
                IsOverdue = dist.DueDate < now
            });
        }

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId && ic.UnitId == unitId && ic.BalanceAmount > 0 && !ic.IsDisputed)
            .OrderBy(ic => ic.ChargeDate)
            .ToListAsync();

        foreach (var charge in overdueCharges)
        {
            items.Add(new DebtItemDto
            {
                SourceType = "IndividualCharge",
                SourceId = charge.Id,
                Description = charge.Concept,
                DueDate = charge.ChargeDate,
                Amount = charge.Amount,
                Balance = charge.BalanceAmount,
                IsOverdue = charge.ChargeDate < now
            });
        }

        var adjustmentTotal = await _context.BillingAdjustments
            .Where(a => a.TenantId == tenantId && a.UnitId == unitId)
            .SumAsync(a => a.Amount);

        var totalDebt = items.Sum(i => i.Balance) + adjustmentTotal;
        var totalOverdue = items.Where(i => i.IsOverdue).Sum(i => i.Balance) + adjustmentTotal;

        var advanceBalance = await GetAdvanceBalanceAsync(tenantId, unitId);

        return new UnitDebtSummaryDto
        {
            UnitId = unitId,
            UnitIdentifier = unit.Identifier,
            TotalDebt = totalDebt,
            TotalOverdue = totalOverdue,
            AdvanceBalance = advanceBalance,
            Items = items
        };
    }

    public async Task<PaymentPreviewDto> PreviewPaymentAllocationAsync(
        string tenantId, Guid unitId, decimal amount)
    {
        var now = DateTime.UtcNow;
        var remaining = amount;
        var allocations = new List<PaymentAllocationPreviewDto>();

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                      && uf.UnitId == unitId
                      && uf.BalanceAmount > 0
                      && uf.DueDate < now)
            .OrderBy(uf => uf.DueDate)
            .ToListAsync();

        foreach (var fee in overdueFees)
        {
            if (remaining <= 0m) break;

            var toAllocate = Math.Min(remaining, fee.BalanceAmount);
            remaining -= toAllocate;

            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = "UnitFee",
                SourceId = fee.Id,
                Description = "Cuota ordinaria " + fee.DueDate.ToString("yyyy-MM"),
                AllocatedAmount = toAllocate
            });
        }

        var overdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId
                      && ed.UnitId == unitId
                      && ed.BalanceAmount > 0
                      && ed.DueDate < now)
            .OrderBy(ed => ed.DueDate)
            .ToListAsync();

        foreach (var dist in overdueExtraordinary)
        {
            if (remaining <= 0m) break;

            var toAllocate = Math.Min(remaining, dist.BalanceAmount);
            remaining -= toAllocate;

            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = "ExtraordinaryFee",
                SourceId = dist.Id,
                Description = "Cuota extraordinaria #" + dist.InstallmentNumber,
                AllocatedAmount = toAllocate
            });
        }

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                      && ic.UnitId == unitId
                      && ic.BalanceAmount > 0
                      && !ic.IsDisputed)
            .OrderBy(ic => ic.ChargeDate)
            .ToListAsync();

        foreach (var charge in overdueCharges)
        {
            if (remaining <= 0m) break;

            var toAllocate = Math.Min(remaining, charge.BalanceAmount);
            remaining -= toAllocate;

            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = "IndividualCharge",
                SourceId = charge.Id,
                Description = charge.Concept,
                AllocatedAmount = toAllocate
            });
        }

        var currentFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                      && uf.UnitId == unitId
                      && uf.BalanceAmount > 0
                      && uf.DueDate >= now)
            .OrderBy(uf => uf.DueDate)
            .ToListAsync();

        foreach (var fee in currentFees)
        {
            if (remaining <= 0m) break;

            var toAllocate = Math.Min(remaining, fee.BalanceAmount);
            remaining -= toAllocate;

            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = "UnitFee",
                SourceId = fee.Id,
                Description = "Cuota corriente " + fee.DueDate.ToString("yyyy-MM"),
                AllocatedAmount = toAllocate
            });
        }

        var totalAllocated = allocations.Sum(a => a.AllocatedAmount);

        return new PaymentPreviewDto
        {
            TotalPayment = amount,
            TotalAllocated = totalAllocated,
            AdvanceAmount = Math.Max(0, remaining),
            Allocations = allocations
        };
    }

    public async Task<Payment> RegisterPaymentAsync(
        string tenantId, RegisterPaymentRequestDto request, string userId)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new ArgumentException("El medio de pago especificado es inválido (Cash, Transfer, Check).");
        }

        var preview = await PreviewPaymentAllocationAsync(tenantId, request.UnitId, request.Amount);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = request.UnitId,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            ReceivedByUserId = userId,
            AdvanceAmount = preview.AdvanceAmount
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        foreach (var alloc in preview.Allocations)
        {
            var allocation = new PaymentAllocation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentId = payment.Id,
                Amount = alloc.AllocatedAmount,
                AllocationType = PaymentAllocationType.Capital
            };

            switch (alloc.SourceType)
            {
                case "UnitFee":
                    allocation.UnitFeeId = alloc.SourceId;
                    await UpdateUnitFeeAfterPayment(alloc.SourceId, alloc.AllocatedAmount);
                    break;
                case "ExtraordinaryFee":
                    allocation.ExtraordinaryFeeDistributionId = alloc.SourceId;
                    await UpdateExtraordinaryFeeAfterPayment(alloc.SourceId, alloc.AllocatedAmount);
                    break;
                case "IndividualCharge":
                    allocation.IndividualChargeId = alloc.SourceId;
                    await UpdateIndividualChargeAfterPayment(alloc.SourceId, alloc.AllocatedAmount);
                    break;
            }

            _context.PaymentAllocations.Add(allocation);
        }

        await _context.SaveChangesAsync();

        await _indicatorCache.InvalidateAsync(tenantId, DashboardService.CollectionChartCacheKeyPrefix);
        await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);
        await SendPaymentConfirmedNotificationAsync(tenantId, payment);
        return payment;
    }

    private async Task SendPaymentConfirmedNotificationAsync(string tenantId, Payment payment)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == payment.UnitId && u.TenantId == tenantId);
        var unitIdentifier = string.Empty;
        if (unit != null)
        {
            unitIdentifier = unit.Identifier;
        }

        var variables = new Dictionary<string, string>
        {
            ["Amount"] = payment.Amount.ToString("N0"),
            ["UnitIdentifier"] = unitIdentifier,
            ["PaymentDate"] = payment.PaymentDate.ToString("dd/MM/yyyy")
        };

        var activeOwner = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId && uo.UnitId == payment.UnitId && uo.IsActive)
            .Select(uo => new { uo.OwnerId, uo.Owner!.FullNameOrCompanyName })
            .FirstOrDefaultAsync();

        if (activeOwner != null)
        {
            var ownerVariables = new Dictionary<string, string>(variables)
            {
                ["ResidentName"] = activeOwner.FullNameOrCompanyName
            };
            await _notificationEngine.ProcessEventAsync(
                tenantId, NotificationEventType.PaymentConfirmed,
                "Billing", payment.Id.ToString(), "Payment",
                ownerId: activeOwner.OwnerId, variables: ownerVariables);
        }

        var activeResident = await _context.TenantResidents
            .Where(tr => tr.TenantId == tenantId && tr.UnitId == payment.UnitId && tr.IsActive)
            .FirstOrDefaultAsync();

        if (activeResident != null)
        {
            var residentVariables = new Dictionary<string, string>(variables)
            {
                ["ResidentName"] = activeResident.FullName
            };
            await _notificationEngine.ProcessEventAsync(
                tenantId, NotificationEventType.PaymentConfirmed,
                "Billing", payment.Id.ToString(), "Payment",
                tenantResidentId: activeResident.Id, variables: residentVariables);
        }
    }

    public async Task<List<PaymentDto>> GetUnitPaymentsAsync(string tenantId, Guid unitId)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId);

        var payments = await _context.Payments
            .Where(p => p.TenantId == tenantId && p.UnitId == unitId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                UnitId = p.UnitId,
                UnitIdentifier = unit != null ? unit.Identifier : string.Empty,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes,
                AdvanceAmount = p.AdvanceAmount,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return payments;
    }

    public async Task<PaymentDetailDto> GetPaymentDetailAsync(string tenantId, Guid paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.TenantId == tenantId);

        if (payment == null)
        {
            throw new KeyNotFoundException("No se encontró el pago.");
        }

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == payment.UnitId);

        var allocations = await _context.PaymentAllocations
            .Where(pa => pa.PaymentId == paymentId)
            .Select(pa => new PaymentAllocationDto
            {
                Id = pa.Id,
                SourceType = pa.UnitFeeId != null ? "UnitFee"
                           : pa.ExtraordinaryFeeDistributionId != null ? "ExtraordinaryFee"
                           : pa.IndividualChargeId != null ? "IndividualCharge"
                           : "Unknown",
                SourceId = pa.UnitFeeId ?? pa.ExtraordinaryFeeDistributionId ?? pa.IndividualChargeId,
                Amount = pa.Amount,
                AllocationType = pa.AllocationType.ToString()
            })
            .ToListAsync();

        return new PaymentDetailDto
        {
            Id = payment.Id,
            UnitId = payment.UnitId,
            UnitIdentifier = unit?.Identifier ?? string.Empty,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            AdvanceAmount = payment.AdvanceAmount,
            CreatedAt = payment.CreatedAt,
            Allocations = allocations
        };
    }

    private async Task UpdateUnitFeeAfterPayment(Guid unitFeeId, decimal paidAmount)
    {
        var fee = await _context.UnitFees.FindAsync(unitFeeId);
        if (fee == null) return;

        fee.PaidAmount += paidAmount;
        fee.BalanceAmount = fee.FeeValue - fee.PaidAmount;

        if (fee.BalanceAmount <= 0m)
        {
            fee.Status = FeeStatus.FullyPaid;
            fee.BalanceAmount = 0m;
        }
        else if (fee.PaidAmount > 0m)
        {
            fee.Status = FeeStatus.PartiallyPaid;
        }
    }

    private async Task UpdateExtraordinaryFeeAfterPayment(Guid distributionId, decimal paidAmount)
    {
        var dist = await _context.ExtraordinaryFeeDistributions.FindAsync(distributionId);
        if (dist == null) return;

        dist.PaidAmount += paidAmount;
        dist.BalanceAmount = dist.Amount - dist.PaidAmount;

        if (dist.BalanceAmount <= 0m)
        {
            dist.Status = FeeStatus.FullyPaid;
            dist.BalanceAmount = 0m;
        }
        else if (dist.PaidAmount > 0m)
        {
            dist.Status = FeeStatus.PartiallyPaid;
        }
    }

    private async Task UpdateIndividualChargeAfterPayment(Guid chargeId, decimal paidAmount)
    {
        var charge = await _context.IndividualCharges.FindAsync(chargeId);
        if (charge == null) return;

        charge.PaidAmount += paidAmount;
        charge.BalanceAmount = charge.Amount - charge.PaidAmount;

        if (charge.BalanceAmount <= 0m)
        {
            charge.Status = IndividualChargeStatus.Paid;
            charge.BalanceAmount = 0m;
        }
    }

    private async Task<decimal> GetAdvanceBalanceAsync(string tenantId, Guid unitId)
    {
        var totalAdvances = await _context.Payments
            .Where(p => p.TenantId == tenantId && p.UnitId == unitId)
            .SumAsync(p => p.AdvanceAmount);

        return totalAdvances;
    }
}
