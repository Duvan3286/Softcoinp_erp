using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class PaymentAgreementService
{
    private readonly ApplicationDbContext _context;

    public PaymentAgreementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public AgreementSimulationDto SimulateAgreement(
        decimal totalDebt,
        int numberOfInstallments,
        decimal interestForgivenessPercentage,
        DateTime startDate)
    {
        var forgivenAmount = Math.Round(totalDebt * (interestForgivenessPercentage / 100m), 2);
        var netDebt = totalDebt - forgivenAmount;
        var installmentAmount = Math.Round(netDebt / numberOfInstallments, 2);

        var installments = new List<SimulatedInstallmentDto>();
        for (var i = 0; i < numberOfInstallments; i++)
        {
            installments.Add(new SimulatedInstallmentDto
            {
                Number = i + 1,
                DueDate = startDate.AddMonths(i),
                Amount = installmentAmount
            });
        }

        var remainder = netDebt - (installmentAmount * numberOfInstallments);
        if (remainder > 0)
        {
            installments[numberOfInstallments - 1].Amount += remainder;
        }

        return new AgreementSimulationDto
        {
            TotalDebt = totalDebt,
            InterestForgivenessPercentage = interestForgivenessPercentage,
            ForgivenAmount = forgivenAmount,
            NetDebt = netDebt,
            NumberOfInstallments = numberOfInstallments,
            InstallmentAmount = installmentAmount,
            Installments = installments
        };
    }

    public async Task<PaymentAgreement> CreateAgreementAsync(
        string tenantId,
        CreatePaymentAgreementRequestDto request,
        string userId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == request.UnitId && u.TenantId == tenantId);

        if (unit == null)
        {
            throw new KeyNotFoundException("No se encontró la unidad especificada.");
        }

        var activeAgreement = await _context.PaymentAgreements
            .AnyAsync(pa => pa.TenantId == tenantId
                         && pa.UnitId == request.UnitId
                         && pa.Status == AgreementStatus.Active);

        if (activeAgreement)
        {
            throw new InvalidOperationException("La unidad ya tiene un acuerdo de pago activo.");
        }

        if (request.NumberOfInstallments < 1)
        {
            throw new ArgumentException("El número de cuotas debe ser al menos 1.");
        }

        if (request.TotalDebtIncluded <= 0)
        {
            throw new ArgumentException("La deuda total incluida debe ser mayor a cero.");
        }

        // Validar que las deudas especificadas existan y pertenezcan a la unidad
        var validatedBalance = 0m;
        foreach (var debt in request.IncludedDebts)
        {
            switch (debt.SourceType)
            {
                case "UnitFee":
                    var fee = await _context.UnitFees
                        .FirstOrDefaultAsync(uf => uf.Id == debt.SourceId && uf.TenantId == tenantId && uf.UnitId == request.UnitId);
                    if (fee == null)
                        throw new ArgumentException($"La cuota ordinaria {debt.SourceId} no existe o no pertenece a la unidad.");
                    if (fee.BalanceAmount <= 0)
                        throw new ArgumentException($"La cuota ordinaria {debt.SourceId} no tiene saldo pendiente.");
                    validatedBalance += fee.BalanceAmount;
                    break;

                case "ExtraordinaryFee":
                    var ed = await _context.ExtraordinaryFeeDistributions
                        .FirstOrDefaultAsync(e => e.Id == debt.SourceId && e.TenantId == tenantId && e.UnitId == request.UnitId);
                    if (ed == null)
                        throw new ArgumentException($"La cuota extraordinaria {debt.SourceId} no existe o no pertenece a la unidad.");
                    if (ed.BalanceAmount <= 0)
                        throw new ArgumentException($"La cuota extraordinaria {debt.SourceId} no tiene saldo pendiente.");
                    validatedBalance += ed.BalanceAmount;
                    break;

                case "IndividualCharge":
                    var charge = await _context.IndividualCharges
                        .FirstOrDefaultAsync(ic => ic.Id == debt.SourceId && ic.TenantId == tenantId && ic.UnitId == request.UnitId);
                    if (charge == null)
                        throw new ArgumentException($"El cobro individual {debt.SourceId} no existe o no pertenece a la unidad.");
                    if (charge.BalanceAmount <= 0)
                        throw new ArgumentException($"El cobro individual {debt.SourceId} no tiene saldo pendiente.");
                    if (charge.IsDisputed)
                        throw new ArgumentException($"El cobro individual {debt.SourceId} está disputado y no puede incluirse en el acuerdo.");
                    validatedBalance += charge.BalanceAmount;
                    break;

                default:
                    throw new ArgumentException($"Tipo de origen inválido: {debt.SourceType}");
            }
        }

        var simulation = SimulateAgreement(
            request.TotalDebtIncluded,
            request.NumberOfInstallments,
            request.InterestForgivenessPercentage,
            request.StartDate);

        var agreement = new PaymentAgreement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = request.UnitId,
            TotalDebtIncluded = request.TotalDebtIncluded,
            InstallmentAmount = simulation.InstallmentAmount,
            NumberOfInstallments = request.NumberOfInstallments,
            InterestForgivenessPercentage = request.InterestForgivenessPercentage,
            CouncilActNumber = request.CouncilActNumber,
            Status = AgreementStatus.Active,
            StartedAt = request.StartDate,
            DigitalAcceptance = request.DigitalAcceptance,
            CreatedByUserId = userId
        };

        _context.PaymentAgreements.Add(agreement);
        await _context.SaveChangesAsync();

        foreach (var sim in simulation.Installments)
        {
            var installment = new AgreementInstallment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentAgreementId = agreement.Id,
                InstallmentNumber = sim.Number,
                DueDate = sim.DueDate,
                Amount = sim.Amount,
                PaidAmount = 0m,
                Status = AgreementInstallmentStatus.Pending
            };

            _context.AgreementInstallments.Add(installment);
        }

        // Persist vínculos con las deudas subyacentes
        foreach (var debt in request.IncludedDebts)
        {
            var originalBalance = 0m;

            if (debt.SourceType == "UnitFee")
            {
                var fee = await _context.UnitFees.FirstOrDefaultAsync(uf => uf.Id == debt.SourceId);
                if (fee != null) originalBalance = fee.BalanceAmount;
            }
            else if (debt.SourceType == "ExtraordinaryFee")
            {
                var ed = await _context.ExtraordinaryFeeDistributions.FirstOrDefaultAsync(e => e.Id == debt.SourceId);
                if (ed != null) originalBalance = ed.BalanceAmount;
            }
            else if (debt.SourceType == "IndividualCharge")
            {
                var charge = await _context.IndividualCharges.FirstOrDefaultAsync(ic => ic.Id == debt.SourceId);
                if (charge != null) originalBalance = charge.BalanceAmount;
            }

            _context.AgreementDebts.Add(new AgreementDebt
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentAgreementId = agreement.Id,
                SourceType = debt.SourceType,
                SourceId = debt.SourceId,
                OriginalBalance = originalBalance
            });
        }

        await _context.SaveChangesAsync();
        return agreement;
    }

    public async Task CheckForDefaultsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var gracePeriodDays = 5;

        var overdueInstallments = await _context.AgreementInstallments
            .Where(ai => ai.TenantId == tenantId
                      && ai.Status == AgreementInstallmentStatus.Pending
                      && ai.DueDate < now.AddDays(-gracePeriodDays))
            .Include(ai => ai.PaymentAgreement)
            .ToListAsync();

        var defaultedAgreementIds = overdueInstallments
            .Select(ai => ai.PaymentAgreementId)
            .Distinct()
            .ToList();

        foreach (var agreementId in defaultedAgreementIds)
        {
            var agreement = await _context.PaymentAgreements
                .FirstOrDefaultAsync(pa => pa.Id == agreementId && pa.Status == AgreementStatus.Active);

            if (agreement == null) continue;

            agreement.Status = AgreementStatus.Defaulted;
            agreement.DefaultedAt = now;

            var pendingInstallments = await _context.AgreementInstallments
                .Where(ai => ai.PaymentAgreementId == agreementId
                          && ai.Status == AgreementInstallmentStatus.Pending)
                .ToListAsync();

            foreach (var inst in pendingInstallments)
            {
                if (inst.DueDate < now)
                {
                    inst.Status = AgreementInstallmentStatus.Overdue;
                }
            }
        }

        if (defaultedAgreementIds.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task ApplyPaymentToAgreementAsync(
        string tenantId, Guid agreementId, decimal amount)
    {
        var agreement = await _context.PaymentAgreements
            .Include(pa => pa.Installments)
            .FirstOrDefaultAsync(pa => pa.Id == agreementId && pa.TenantId == tenantId);

        if (agreement == null)
        {
            throw new KeyNotFoundException("No se encontró el acuerdo de pago.");
        }

        if (agreement.Status != AgreementStatus.Active)
        {
            throw new InvalidOperationException("El acuerdo no está activo.");
        }

        var remaining = amount;
        var pendingInstallments = agreement.Installments
            .Where(i => i.Status == AgreementInstallmentStatus.Pending)
            .OrderBy(i => i.InstallmentNumber)
            .ToList();

        foreach (var installment in pendingInstallments)
        {
            if (remaining <= 0m) break;

            var owed = installment.Amount - installment.PaidAmount;
            var toPay = Math.Min(remaining, owed);
            remaining -= toPay;

            installment.PaidAmount += toPay;
            installment.PaidAt = DateTime.UtcNow;

            if (Math.Abs(installment.PaidAmount - installment.Amount) < 0.01m)
            {
                installment.Status = AgreementInstallmentStatus.Paid;
            }
        }

        var allPaid = agreement.Installments.All(i => i.Status == AgreementInstallmentStatus.Paid);
        if (allPaid)
        {
            agreement.Status = AgreementStatus.Completed;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<PaymentAgreementSummaryDto>> GetActiveAgreementsAsync(string tenantId)
    {
        await CheckForDefaultsAsync(tenantId);

        var agreements = await _context.PaymentAgreements
            .Where(pa => pa.TenantId == tenantId)
            .OrderByDescending(pa => pa.StartedAt)
            .ToListAsync();

        var unitIds = agreements.Select(a => a.UnitId).Distinct().ToList();
        var units = await _context.Units
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = new List<PaymentAgreementSummaryDto>();

        foreach (var agreement in agreements)
        {
            var installments = await _context.AgreementInstallments
                .Where(ai => ai.PaymentAgreementId == agreement.Id)
                .ToListAsync();

            result.Add(new PaymentAgreementSummaryDto
            {
                Id = agreement.Id,
                UnitId = agreement.UnitId,
                UnitIdentifier = units.GetValueOrDefault(agreement.UnitId)?.Identifier ?? string.Empty,
                TotalDebtIncluded = agreement.TotalDebtIncluded,
                InstallmentAmount = agreement.InstallmentAmount,
                NumberOfInstallments = agreement.NumberOfInstallments,
                InterestForgivenessPercentage = agreement.InterestForgivenessPercentage,
                Status = agreement.Status.ToString(),
                StartedAt = agreement.StartedAt,
                DefaultedAt = agreement.DefaultedAt,
                PaidInstallments = installments.Count(i => i.Status == AgreementInstallmentStatus.Paid),
                OverdueInstallments = installments.Count(i => i.Status == AgreementInstallmentStatus.Overdue)
            });
        }

        return result;
    }

    public async Task<PaymentAgreementDetailDto> GetAgreementDetailAsync(
        string tenantId, Guid agreementId)
    {
        var agreement = await _context.PaymentAgreements
            .FirstOrDefaultAsync(pa => pa.Id == agreementId && pa.TenantId == tenantId);

        if (agreement == null)
        {
            throw new KeyNotFoundException("No se encontró el acuerdo de pago.");
        }

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == agreement.UnitId);

        var installments = await _context.AgreementInstallments
            .Where(ai => ai.PaymentAgreementId == agreementId)
            .OrderBy(ai => ai.InstallmentNumber)
            .Select(ai => new AgreementInstallmentDto
            {
                Id = ai.Id,
                InstallmentNumber = ai.InstallmentNumber,
                DueDate = ai.DueDate,
                Amount = ai.Amount,
                PaidAmount = ai.PaidAmount,
                Status = ai.Status.ToString(),
                PaidAt = ai.PaidAt
            })
            .ToListAsync();

        var includedDebts = await _context.AgreementDebts
            .Where(ad => ad.PaymentAgreementId == agreementId)
            .Select(ad => new AgreementDebtItemDto
            {
                SourceType = ad.SourceType,
                SourceId = ad.SourceId
            })
            .ToListAsync();

        return new PaymentAgreementDetailDto
        {
            Id = agreement.Id,
            UnitId = agreement.UnitId,
            UnitIdentifier = unit?.Identifier ?? string.Empty,
            TotalDebtIncluded = agreement.TotalDebtIncluded,
            InstallmentAmount = agreement.InstallmentAmount,
            NumberOfInstallments = agreement.NumberOfInstallments,
            InterestForgivenessPercentage = agreement.InterestForgivenessPercentage,
            CouncilActNumber = agreement.CouncilActNumber,
            Status = agreement.Status.ToString(),
            StartedAt = agreement.StartedAt,
            DefaultedAt = agreement.DefaultedAt,
            DigitalAcceptance = agreement.DigitalAcceptance,
            Installments = installments,
            IncludedDebts = includedDebts
        };
    }
}
