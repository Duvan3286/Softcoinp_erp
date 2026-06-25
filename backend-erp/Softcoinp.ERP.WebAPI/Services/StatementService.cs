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

public class StatementService
{
    private readonly ApplicationDbContext _context;
    private readonly LateInterestService _lateInterestService;
    private readonly AccountingIntegrationService _accounting;

    public StatementService(
        ApplicationDbContext context,
        LateInterestService lateInterestService,
        AccountingIntegrationService accounting)
    {
        _context = context;
        _lateInterestService = lateInterestService;
        _accounting = accounting;
    }

    public async Task<UnitStatementDto> GetUnitStatementAsync(
        string tenantId, Guid unitId, DateTime? startDate, DateTime? endDate)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
        {
            throw new KeyNotFoundException("No se encontró la unidad.");
        }

        var periodStart = startDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var periodEnd = endDate ?? DateTime.UtcNow;

        var lines = new List<StatementLineDto>();
        var runningBalance = 0m;

        var allCharges = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.UnitId == unitId)
            .OrderBy(uf => uf.DueDate)
            .ToListAsync();

        var allExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId && ed.UnitId == unitId)
            .OrderBy(ed => ed.DueDate)
            .ToListAsync();

        var allCharges_ind = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId && ic.UnitId == unitId)
            .OrderBy(ic => ic.ChargeDate)
            .ToListAsync();

        var allPayments = await _context.Payments
            .Where(p => p.TenantId == tenantId && p.UnitId == unitId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

        var allCapitalizedInterests = await _context.LateInterests
            .Where(li => li.TenantId == tenantId && li.IsCapitalized
                      && _context.UnitFees.Any(uf => uf.Id == li.UnitFeeId && uf.UnitId == unitId))
            .OrderBy(li => li.Period)
            .ToListAsync();

        var chargeLines = new List<(DateTime Date, string Desc, string Ref, decimal Debit, decimal Credit)>();

        foreach (var fee in allCharges)
        {
            chargeLines.Add((fee.DueDate, "Cuota ordinaria " + fee.DueDate.ToString("yyyy-MM"),
                fee.Id.ToString(), fee.FeeValue, 0m));
        }

        foreach (var ed in allExtraordinary)
        {
            chargeLines.Add((ed.DueDate, "Cuota extraordinaria #" + ed.InstallmentNumber,
                ed.ExtraordinaryFeeId.ToString(), ed.Amount, 0m));
        }

        foreach (var ic in allCharges_ind)
        {
            chargeLines.Add((ic.ChargeDate, ic.Concept, ic.Id.ToString(), ic.Amount, 0m));
        }

        foreach (var li in allCapitalizedInterests)
        {
            chargeLines.Add((DateTime.Parse(li.Period + "-01"), "Interés mora " + li.Period,
                li.Id.ToString(), li.CalculatedAmount, 0m));
        }

        foreach (var p in allPayments)
        {
            chargeLines.Add((p.PaymentDate, "Pago " + p.PaymentMethod,
                p.ReferenceNumber, 0m, p.Amount));
        }

        chargeLines = chargeLines.OrderBy(c => c.Date).ToList();

        var openingBalance = 0m;
        foreach (var (date, desc, ref_, debit, credit) in chargeLines)
        {
            if (date < periodStart)
            {
                openingBalance += debit - credit;
            }
        }

        runningBalance = openingBalance;

        foreach (var (date, desc, ref_, debit, credit) in chargeLines)
        {
            if (date < periodStart) continue;
            if (date > periodEnd) break;

            runningBalance += debit - credit;

            lines.Add(new StatementLineDto
            {
                Date = date,
                Description = desc,
                Reference = ref_,
                Debit = debit,
                Credit = credit,
                Balance = runningBalance
            });
        }

        var realTimeInterests = await _lateInterestService.PreviewUnitInterestAsync(
            tenantId, unitId, periodEnd);

        var totalInterest = realTimeInterests.Sum(i => i.CalculatedInterest);
        var closingBalance = runningBalance + totalInterest;

        return new UnitStatementDto
        {
            UnitId = unitId,
            UnitIdentifier = unit.Identifier,
            UnitTower = unit.TowerOrBlock,
            OpeningBalance = openingBalance,
            TotalCharges = chargeLines.Where(c => c.Date >= periodStart && c.Date <= periodEnd).Sum(c => c.Debit),
            TotalPayments = chargeLines.Where(c => c.Date >= periodStart && c.Date <= periodEnd).Sum(c => c.Credit),
            TotalInterest = totalInterest,
            ClosingBalance = closingBalance,
            Lines = lines
        };
    }

    public async Task<ClearanceCertificate> IssueClearanceCertificateAsync(
        string tenantId, Guid unitId, int validityDays, string userId)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
        {
            throw new KeyNotFoundException("No se encontró la unidad.");
        }

        var statement = await GetUnitStatementAsync(tenantId, unitId, null, DateTime.UtcNow);

        if (statement.ClosingBalance > 0)
        {
            throw new InvalidOperationException(
                "No se puede expedir paz y salvo. La unidad tiene un saldo pendiente de " +
                statement.ClosingBalance.ToString("C2") + ".");
        }

        var config = await _context.TenantConfigurations.FirstOrDefaultAsync();
        var administratorName = config?.LegalRepresentativeName ?? "Administrador";

        var lastCert = await _context.ClearanceCertificates
            .Where(cc => cc.TenantId == tenantId)
            .OrderByDescending(cc => cc.CertificateNumber)
            .FirstOrDefaultAsync();

        var lastNumber = 0;
        if (lastCert != null && int.TryParse(lastCert.CertificateNumber, out var parsed))
        {
            lastNumber = parsed;
        }

        var certificate = new ClearanceCertificate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            CertificateNumber = (lastNumber + 1).ToString("D6"),
            IssueDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(validityDays),
            BalanceAtDate = statement.ClosingBalance,
            Status = ClearanceCertificateStatus.Active,
            IssuedByUserId = userId,
            SignedByAdministratorName = administratorName
        };

        _context.ClearanceCertificates.Add(certificate);
        await _context.SaveChangesAsync();

        try
        {
            await _accounting.RecordClearanceCertificateAsync(
                tenantId,
                certificate.Id,
                certificate.CertificateNumber,
                unit.Identifier,
                statement.ClosingBalance,
                $"Paz y salvo expedido para unidad {unit.Identifier} - Certificado {certificate.CertificateNumber}",
                userId);
        }
        catch (Exception ex)
        {
            _ = ex;
        }

        return certificate;
    }

    public async Task<List<ClearanceCertificateSummaryDto>> GetUnitCertificatesAsync(
        string tenantId, Guid unitId)
    {
        var certificates = await _context.ClearanceCertificates
            .Where(cc => cc.TenantId == tenantId && cc.UnitId == unitId)
            .OrderByDescending(cc => cc.IssueDate)
            .Join(_context.Units,
                  cc => cc.UnitId,
                  u => u.Id,
                  (cc, u) => new ClearanceCertificateSummaryDto
                  {
                      Id = cc.Id,
                      UnitId = cc.UnitId,
                      UnitIdentifier = u.Identifier,
                      CertificateNumber = cc.CertificateNumber,
                      IssueDate = cc.IssueDate,
                      ExpirationDate = cc.ExpirationDate,
                      Status = cc.Status.ToString()
                  })
            .ToListAsync();

        return certificates;
    }

    public async Task<ClearanceCertificateDto> GetCertificateDetailAsync(
        string tenantId, Guid certificateId)
    {
        var cert = await _context.ClearanceCertificates
            .FirstOrDefaultAsync(cc => cc.Id == certificateId && cc.TenantId == tenantId);

        if (cert == null)
        {
            throw new KeyNotFoundException("No se encontró el paz y salvo.");
        }

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == cert.UnitId);

        return new ClearanceCertificateDto
        {
            Id = cert.Id,
            UnitId = cert.UnitId,
            UnitIdentifier = unit?.Identifier ?? string.Empty,
            CertificateNumber = cert.CertificateNumber,
            IssueDate = cert.IssueDate,
            ExpirationDate = cert.ExpirationDate,
            BalanceAtDate = cert.BalanceAtDate,
            Status = cert.Status.ToString(),
            IssuedByUserId = cert.IssuedByUserId,
            SignedByAdministratorName = cert.SignedByAdministratorName
        };
    }

    public async Task RevokeCertificateAsync(string tenantId, Guid certificateId)
    {
        var cert = await _context.ClearanceCertificates
            .FirstOrDefaultAsync(cc => cc.Id == certificateId && cc.TenantId == tenantId);

        if (cert == null)
        {
            throw new KeyNotFoundException("No se encontró el paz y salvo.");
        }

        if (cert.Status != ClearanceCertificateStatus.Active)
        {
            throw new InvalidOperationException("Solo se puede revocar un paz y salvo activo.");
        }

        cert.Status = ClearanceCertificateStatus.Revoked;
        await _context.SaveChangesAsync();
    }
}
