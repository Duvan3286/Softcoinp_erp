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
    private readonly InterestCalculationService _interestCalculation;

    public PaymentService(
        ApplicationDbContext context,
        ILogger<PaymentService> logger,
        IndicatorCacheService indicatorCache,
        NotificationEngine notificationEngine,
        InterestCalculationService interestCalculation)
    {
        _context = context;
        _logger = logger;
        _indicatorCache = indicatorCache;
        _notificationEngine = notificationEngine;
        _interestCalculation = interestCalculation;
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

    public async Task<PaymentPreviewDto> PreviewManualPaymentAsync(
        string tenantId, Guid unitId, List<ManualAllocationLineDto> lines)
    {
        var allocations = new List<PaymentAllocationPreviewDto>();
        decimal totalAllocated = 0m;

        foreach (var line in lines)
        {
            string description;
            Guid? accruedInterestId = null;

            switch (line.SourceType)
            {
                case "UnitFee":
                    var fee = await _context.UnitFees.FindAsync(line.SourceId);
                    description = fee != null
                        ? "Cuota ordinaria " + fee.DueDate.ToString("yyyy-MM")
                        : "Cuota ordinaria";
                    break;
                case "ExtraordinaryFee":
                    var dist = await _context.ExtraordinaryFeeDistributions.FindAsync(line.SourceId);
                    description = dist != null
                        ? "Cuota extraordinaria #" + dist.InstallmentNumber
                        : "Cuota extraordinaria";
                    break;
                case "IndividualCharge":
                    var charge = await _context.IndividualCharges.FindAsync(line.SourceId);
                    description = charge?.Concept ?? "Cargo individual";
                    break;
                case "Interest":
                    var interest = await _context.AccruedInterests.FindAsync(line.SourceId);
                    description = interest != null
                        ? "Interés mora " + interest.Period
                        : "Interés mora";
                    accruedInterestId = line.SourceId;
                    break;
                default:
                    description = line.SourceType;
                    break;
            }

            totalAllocated += line.Amount;
            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = line.SourceType,
                SourceId = line.SourceId,
                Description = description,
                AllocatedAmount = line.Amount,
                AccruedInterestId = accruedInterestId
            });
        }

        return new PaymentPreviewDto
        {
            TotalPayment = totalAllocated,
            TotalAllocated = totalAllocated,
            AdvanceAmount = 0m,
            Allocations = allocations
        };
    }

    public async Task<PaymentPreviewDto> PreviewPaymentAllocationAsync(
        string tenantId, Guid unitId, decimal amount)
    {
        var remaining = amount;
        var allocations = new List<PaymentAllocationPreviewDto>();

        remaining = await AllocateToInterestAsync(tenantId, unitId, remaining, allocations);

        if (remaining > 0m)
        {
            remaining = await AllocateToCapitalAsync(tenantId, unitId, remaining, allocations);
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

    private async Task<decimal> AllocateToInterestAsync(
        string tenantId, Guid unitId, decimal remaining, List<PaymentAllocationPreviewDto> allocations)
    {
        var pendingInterests = await _context.AccruedInterests
            .Where(ai => ai.TenantId == tenantId
                      && ai.UnitId == unitId
                      && ai.BalanceAmount > 0
                      && ai.Status == AccruedInterestStatus.Pending)
            .OrderBy(ai => ai.InterestStartDate)
            .ThenBy(ai => ai.Period)
            .ToListAsync();

        foreach (var interest in pendingInterests)
        {
            if (remaining <= 0m) break;

            var toAllocate = Math.Min(remaining, interest.BalanceAmount);
            remaining -= toAllocate;

            var description = InteresPeriodDescription(interest);
            allocations.Add(new PaymentAllocationPreviewDto
            {
                SourceType = "Interest",
                SourceId = interest.Id,
                Description = description,
                AllocatedAmount = toAllocate,
                AccruedInterestId = interest.Id
            });
        }

        return remaining;
    }

    private async Task<decimal> AllocateToCapitalAsync(
        string tenantId, Guid unitId, decimal remaining, List<PaymentAllocationPreviewDto> allocations)
    {
        var now = DateTime.UtcNow;

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

        return remaining;
    }

    private static string InteresPeriodDescription(AccruedInterest interest)
    {
        if (interest.UnitFeeId.HasValue)
            return "Interés mora cuota " + interest.Period;
        if (interest.ExtraordinaryFeeDistributionId.HasValue)
            return "Interés mora cuota extra #" + interest.Period;
        if (interest.IndividualChargeId.HasValue)
            return "Interés mora cargo " + interest.Period;
        return "Interés mora " + interest.Period;
    }

    public async Task<Payment> RegisterPaymentAsync(
        string tenantId, RegisterPaymentRequestDto request, string userId)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new ArgumentException("El medio de pago especificado es inválido (Cash, Transfer, Check).");
        }

        if (!Enum.TryParse<ImputationType>(request.ImputationType, true, out var imputationType))
        {
            throw new ArgumentException("El tipo de imputación es inválido (Automatic, Manual).");
        }

        if (imputationType == ImputationType.Manual && string.IsNullOrWhiteSpace(request.ManualJustification))
        {
            throw new ArgumentException("La justificación es obligatoria para imputación manual.");
        }

        await _interestCalculation.CalculateAndSaveInterestsAsync(tenantId, request.UnitId, userId);
        var unitEntry = await _context.Units.FindAsync(request.UnitId);
        if (unitEntry != null)
        {
            await _context.Entry(unitEntry).ReloadAsync();
        }

        if (imputationType == ImputationType.Manual)
        {
            return await RegisterManualPaymentAsync(tenantId, request, paymentMethod, userId);
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
            AdvanceAmount = preview.AdvanceAmount,
            ImputationType = imputationType,
            ManualJustification = request.ManualJustification
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        foreach (var alloc in preview.Allocations)
        {
            var allocationType = alloc.AccruedInterestId.HasValue
                ? PaymentAllocationType.Interest
                : PaymentAllocationType.Capital;

            var allocation = new PaymentAllocation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentId = payment.Id,
                Amount = alloc.AllocatedAmount,
                AllocationType = allocationType,
                AccruedInterestId = alloc.AccruedInterestId
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
                case "Interest":
                    await UpdateAccruedInterestAfterPayment(alloc.AccruedInterestId!.Value, alloc.AllocatedAmount);
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

    private async Task<Payment> RegisterManualPaymentAsync(
        string tenantId, RegisterPaymentRequestDto request, PaymentMethod paymentMethod, string userId)
    {
        if (request.ManualAllocations == null || request.ManualAllocations.Count == 0)
        {
            throw new ArgumentException("Debe especificar al menos una línea de asignación manual.");
        }

        var totalAllocated = request.ManualAllocations.Sum(a => a.Amount);
        if (totalAllocated != request.Amount)
        {
            throw new ArgumentException(
                $"La suma de las asignaciones manuales ({totalAllocated:N2}) no coincide con el monto del pago ({request.Amount:N2}).");
        }

        foreach (var line in request.ManualAllocations)
        {
            switch (line.SourceType)
            {
                case "UnitFee":
                    var fee = await _context.UnitFees
                        .FirstOrDefaultAsync(f => f.Id == line.SourceId && f.TenantId == tenantId && f.UnitId == request.UnitId);
                    if (fee == null)
                        throw new ArgumentException($"La cuota ordinaria {line.SourceId} no existe o no pertenece a la unidad.");
                    if (line.Amount > fee.BalanceAmount)
                        throw new ArgumentException(
                            $"El monto asignado a la cuota ({line.Amount:N2}) supera el saldo pendiente ({fee.BalanceAmount:N2}).");
                    break;

                case "ExtraordinaryFee":
                    var dist = await _context.ExtraordinaryFeeDistributions
                        .FirstOrDefaultAsync(d => d.Id == line.SourceId && d.TenantId == tenantId && d.UnitId == request.UnitId);
                    if (dist == null)
                        throw new ArgumentException($"La cuota extraordinaria {line.SourceId} no existe o no pertenece a la unidad.");
                    if (line.Amount > dist.BalanceAmount)
                        throw new ArgumentException(
                            $"El monto asignado a la cuota extraordinaria ({line.Amount:N2}) supera el saldo pendiente ({dist.BalanceAmount:N2}).");
                    break;

                case "IndividualCharge":
                    var charge = await _context.IndividualCharges
                        .FirstOrDefaultAsync(c => c.Id == line.SourceId && c.TenantId == tenantId && c.UnitId == request.UnitId);
                    if (charge == null)
                        throw new ArgumentException($"El cargo individual {line.SourceId} no existe o no pertenece a la unidad.");
                    if (charge.IsDisputed)
                        throw new ArgumentException($"El cargo individual {line.SourceId} está en disputa y no puede pagarse.");
                    if (line.Amount > charge.BalanceAmount)
                        throw new ArgumentException(
                            $"El monto asignado al cargo ({line.Amount:N2}) supera el saldo pendiente ({charge.BalanceAmount:N2}).");
                    break;

                case "Interest":
                    var accruedInterest = await _context.AccruedInterests
                        .FirstOrDefaultAsync(ai => ai.Id == line.SourceId && ai.TenantId == tenantId && ai.UnitId == request.UnitId);
                    if (accruedInterest == null)
                        throw new ArgumentException($"El interés por mora {line.SourceId} no existe o no pertenece a la unidad.");
                    if (accruedInterest.Status == AccruedInterestStatus.Paid)
                        throw new ArgumentException($"El interés por mora {line.SourceId} ya está pagado.");
                    if (line.Amount > accruedInterest.BalanceAmount)
                        throw new ArgumentException(
                            $"El monto asignado al interés ({line.Amount:N2}) supera el saldo pendiente ({accruedInterest.BalanceAmount:N2}).");
                    break;

                default:
                    throw new ArgumentException(
                        $"El tipo de fuente '{line.SourceType}' no es válido (UnitFee, ExtraordinaryFee, IndividualCharge, Interest).");
            }
        }

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
            AdvanceAmount = 0m,
            ImputationType = ImputationType.Manual,
            ManualJustification = request.ManualJustification
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        foreach (var line in request.ManualAllocations)
        {
            var allocationType = line.SourceType == "Interest"
                ? PaymentAllocationType.Interest
                : PaymentAllocationType.Capital;

            Guid? accruedInterestId = line.SourceType == "Interest" ? line.SourceId : null;

            var allocation = new PaymentAllocation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentId = payment.Id,
                Amount = line.Amount,
                AllocationType = allocationType,
                AccruedInterestId = accruedInterestId
            };

            switch (line.SourceType)
            {
                case "UnitFee":
                    allocation.UnitFeeId = line.SourceId;
                    await UpdateUnitFeeAfterPayment(line.SourceId, line.Amount);
                    break;
                case "ExtraordinaryFee":
                    allocation.ExtraordinaryFeeDistributionId = line.SourceId;
                    await UpdateExtraordinaryFeeAfterPayment(line.SourceId, line.Amount);
                    break;
                case "IndividualCharge":
                    allocation.IndividualChargeId = line.SourceId;
                    await UpdateIndividualChargeAfterPayment(line.SourceId, line.Amount);
                    break;
                case "Interest":
                    await UpdateAccruedInterestAfterPayment(line.SourceId, line.Amount);
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
                SourceType = pa.AccruedInterestId != null ? "Interest"
                           : pa.UnitFeeId != null ? "UnitFee"
                           : pa.ExtraordinaryFeeDistributionId != null ? "ExtraordinaryFee"
                           : pa.IndividualChargeId != null ? "IndividualCharge"
                           : "Unknown",
                SourceId = (Guid?)pa.AccruedInterestId ?? pa.UnitFeeId ?? pa.ExtraordinaryFeeDistributionId ?? pa.IndividualChargeId,
                Amount = pa.Amount,
                AllocationType = pa.AllocationType.ToString(),
                AccruedInterestId = pa.AccruedInterestId
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

    private async Task UpdateAccruedInterestAfterPayment(Guid accruedInterestId, decimal paidAmount)
    {
        var interest = await _context.AccruedInterests.FindAsync(accruedInterestId);
        if (interest == null) return;

        interest.BalanceAmount -= paidAmount;

        if (interest.BalanceAmount <= 0m)
        {
            interest.Status = AccruedInterestStatus.Paid;
            interest.BalanceAmount = 0m;
        }

        interest.UpdatedAt = DateTime.UtcNow;
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
