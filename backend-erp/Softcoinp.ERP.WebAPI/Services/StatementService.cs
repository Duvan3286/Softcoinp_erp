using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class StatementService
{
    private readonly ApplicationDbContext _context;

    public StatementService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
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

        var allCharges = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.UnitId == unitId)
            .OrderBy(uf => uf.DueDate)
            .ToListAsync();

        var allExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId && ed.UnitId == unitId)
            .OrderBy(ed => ed.DueDate)
            .ToListAsync();

        var allIndividualCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId && ic.UnitId == unitId)
            .OrderBy(ic => ic.ChargeDate)
            .ToListAsync();

        var allAdjustments = await _context.BillingAdjustments
            .Where(a => a.TenantId == tenantId && a.UnitId == unitId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var allPayments = await _context.Payments
            .Where(p => p.TenantId == tenantId && p.UnitId == unitId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

        var allInterests = await _context.AccruedInterests
            .Where(ai => ai.TenantId == tenantId && ai.UnitId == unitId)
            .OrderBy(ai => ai.InterestEndDate)
            .ToListAsync();

        var paymentIds = allPayments.Select(p => p.Id).ToList();
        var allAllocations = await _context.PaymentAllocations
            .Where(pa => paymentIds.Contains(pa.PaymentId))
            .ToListAsync();

        var lines = new List<StatementLineEntry>();

        foreach (var fee in allCharges)
        {
            var statusLabel = "Al día";
            if (fee.DueDate < DateTime.UtcNow && fee.BalanceAmount > 0m)
            {
                statusLabel = "Vencida";
            }

            lines.Add(new StatementLineEntry
            {
                Date = fee.DueDate,
                Description = "Cuota ordinaria " + fee.DueDate.ToString("yyyy-MM") + " (" + statusLabel + ")",
                Reference = fee.Id.ToString(),
                Debit = fee.FeeValue,
                Credit = 0m,
                LineType = "Principal"
            });
        }

        foreach (var distribution in allExtraordinary)
        {
            lines.Add(new StatementLineEntry
            {
                Date = distribution.DueDate,
                Description = "Cuota extraordinaria #" + distribution.InstallmentNumber,
                Reference = distribution.ExtraordinaryFeeId.ToString(),
                Debit = distribution.Amount,
                Credit = 0m,
                LineType = "Principal"
            });
        }

        foreach (var charge in allIndividualCharges)
        {
            lines.Add(new StatementLineEntry
            {
                Date = charge.ChargeDate,
                Description = charge.Concept,
                Reference = charge.Id.ToString(),
                Debit = charge.Amount,
                Credit = 0m,
                LineType = "Principal"
            });
        }

        foreach (var adjustment in allAdjustments)
        {
            var debit = 0m;
            var credit = 0m;
            if (adjustment.Amount >= 0m)
            {
                debit = adjustment.Amount;
            }
            else
            {
                credit = -adjustment.Amount;
            }

            lines.Add(new StatementLineEntry
            {
                Date = adjustment.CreatedAt,
                Description = "Ajuste: " + adjustment.Reason,
                Reference = adjustment.Id.ToString(),
                Debit = debit,
                Credit = credit,
                LineType = "Principal"
            });
        }

        foreach (var interest in allInterests)
        {
            var description = "Interés mora cargo " + interest.Period;
            if (interest.UnitFeeId.HasValue)
            {
                description = "Interés mora cuota " + interest.Period;
            }
            else if (interest.ExtraordinaryFeeDistributionId.HasValue)
            {
                description = "Interés mora cuota extra " + interest.Period;
            }

            lines.Add(new StatementLineEntry
            {
                Date = interest.InterestEndDate,
                Description = description,
                Reference = interest.Id.ToString(),
                Debit = interest.CalculatedAmount,
                Credit = 0m,
                LineType = "Interest",
                Period = interest.Period,
                DailyRate = interest.DailyRate,
                DaysInPeriod = interest.DaysInPeriod,
                BaseAmount = interest.BaseAmount
            });
        }

        foreach (var payment in allPayments)
        {
            var imputationLabel = "Automática";
            if (payment.ImputationType == ImputationType.Manual)
            {
                imputationLabel = "Manual";
            }

            lines.Add(new StatementLineEntry
            {
                Date = payment.PaymentDate,
                Description = "Pago " + payment.PaymentMethod + " (" + imputationLabel + ")",
                Reference = payment.ReferenceNumber,
                Debit = 0m,
                Credit = payment.Amount,
                LineType = "Payment",
                ImputationType = payment.ImputationType.ToString()
            });
        }

        lines = lines.OrderBy(l => l.Date).ToList();

        var openingBalance = 0m;
        var openingInterestBalance = 0m;
        var openingPrincipalBalance = 0m;

        foreach (var line in lines)
        {
            if (line.Date >= periodStart)
            {
                break;
            }

            if (line.LineType == "Interest")
            {
                openingInterestBalance += line.Debit - line.Credit;
            }
            else
            {
                openingPrincipalBalance += line.Debit - line.Credit;
            }
        }

        openingBalance = openingInterestBalance + openingPrincipalBalance;

        var runningBalance = openingBalance;
        var runningInterestBalance = openingInterestBalance;
        var runningPrincipalBalance = openingPrincipalBalance;
        var statementLines = new List<StatementLineDto>();
        var periodInterestCharged = 0m;
        var periodInterestPaid = 0m;
        var periodPrincipalCharged = 0m;
        var periodPrincipalPaid = 0m;

        foreach (var line in lines)
        {
            if (line.Date < periodStart)
            {
                continue;
            }

            if (line.Date > periodEnd)
            {
                break;
            }

            runningBalance += line.Debit - line.Credit;

            if (line.LineType == "Interest")
            {
                runningInterestBalance += line.Debit - line.Credit;
                periodInterestCharged += line.Debit;
                periodInterestPaid += line.Credit;
            }
            else
            {
                runningPrincipalBalance += line.Debit - line.Credit;
                periodPrincipalCharged += line.Debit;
                periodPrincipalPaid += line.Credit;
            }

            statementLines.Add(new StatementLineDto
            {
                Date = line.Date,
                Description = line.Description,
                Reference = line.Reference,
                Debit = line.Debit,
                Credit = line.Credit,
                Balance = runningBalance,
                LineType = line.LineType,
                Period = line.Period,
                DailyRate = line.DailyRate,
                DaysInPeriod = line.DaysInPeriod,
                BaseAmount = line.BaseAmount,
                ImputationType = line.ImputationType
            });
        }

        var totalCharges = periodPrincipalCharged + periodInterestCharged;
        var totalPayments = periodPrincipalPaid + periodInterestPaid;

        return new UnitStatementDto
        {
            UnitId = unitId,
            UnitIdentifier = unit.Identifier,
            UnitTower = unit.TowerOrBlock,
            OpeningBalance = openingBalance,
            TotalCharges = totalCharges,
            TotalPayments = totalPayments,
            ClosingBalance = runningBalance,
            TotalInterestCharged = periodInterestCharged,
            TotalInterestPaid = periodInterestPaid,
            TotalPrincipalCharged = periodPrincipalCharged,
            TotalPrincipalPaid = periodPrincipalPaid,
            InterestBalance = runningInterestBalance,
            PrincipalBalance = runningPrincipalBalance,
            Lines = statementLines
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

        var config = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);
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
        var certificate = await GetCertificateOrThrowAsync(tenantId, certificateId);
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == certificate.UnitId);

        return MapToDto(certificate, unit);
    }

    public async Task RevokeCertificateAsync(string tenantId, Guid certificateId)
    {
        var certificate = await GetCertificateOrThrowAsync(tenantId, certificateId);

        if (certificate.Status != ClearanceCertificateStatus.Active)
        {
            throw new InvalidOperationException("Solo se puede revocar un paz y salvo activo.");
        }

        certificate.Status = ClearanceCertificateStatus.Revoked;
        await _context.SaveChangesAsync();
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(string tenantId, Guid certificateId)
    {
        var certificate = await GetCertificateOrThrowAsync(tenantId, certificateId);
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == certificate.UnitId);
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);

        var conjuntoName = config?.OfficialName ?? string.Empty;
        var conjuntoNit = config?.Nit ?? string.Empty;
        var unitIdentifier = unit?.Identifier ?? string.Empty;
        var unitTower = unit?.TowerOrBlock ?? string.Empty;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(style => style.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().Text(conjuntoName).FontSize(16).Bold();
                    if (!string.IsNullOrEmpty(conjuntoNit))
                    {
                        column.Item().Text("NIT " + conjuntoNit).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                    column.Item().PaddingTop(10).Text("CERTIFICADO DE PAZ Y SALVO").FontSize(14).Bold().AlignCenter();
                    column.Item().Text("No. " + certificate.CertificateNumber).FontSize(11).AlignCenter();
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text(text =>
                    {
                        text.Span("El administrador del conjunto hace constar que la unidad ");
                        text.Span(unitIdentifier).Bold();
                        text.Span(string.IsNullOrEmpty(unitTower) ? "" : " (" + unitTower + ")");
                        text.Span(" se encuentra a paz y salvo por concepto de cuotas de administración, cuotas extraordinarias y cobros individuales, con corte al ");
                        text.Span(certificate.IssueDate.ToString("dd/MM/yyyy")).Bold();
                        text.Span(".");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Saldo pendiente a la fecha de expedición: ");
                        text.Span(certificate.BalanceAtDate.ToString("C2")).Bold();
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Este certificado tiene vigencia hasta el ");
                        text.Span(certificate.ExpirationDate.ToString("dd/MM/yyyy")).Bold();
                        text.Span(". Después de esta fecha debe solicitarse uno nuevo.");
                    });

                    column.Item().PaddingTop(40).Text(certificate.SignedByAdministratorName).Bold();
                    column.Item().Text("Administrador del conjunto");
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generado el " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task<ClearanceCertificate> GetCertificateOrThrowAsync(string tenantId, Guid certificateId)
    {
        var certificate = await _context.ClearanceCertificates
            .FirstOrDefaultAsync(cc => cc.Id == certificateId && cc.TenantId == tenantId);

        if (certificate == null)
        {
            throw new KeyNotFoundException("No se encontró el paz y salvo.");
        }

        return certificate;
    }

    private static ClearanceCertificateDto MapToDto(ClearanceCertificate certificate, Softcoinp.ERP.Domain.Entities.Unit? unit)
    {
        return new ClearanceCertificateDto
        {
            Id = certificate.Id,
            UnitId = certificate.UnitId,
            UnitIdentifier = unit?.Identifier ?? string.Empty,
            CertificateNumber = certificate.CertificateNumber,
            IssueDate = certificate.IssueDate,
            ExpirationDate = certificate.ExpirationDate,
            BalanceAtDate = certificate.BalanceAtDate,
            Status = certificate.Status.ToString(),
            IssuedByUserId = certificate.IssuedByUserId,
            SignedByAdministratorName = certificate.SignedByAdministratorName
        };
    }

    private class StatementLineEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string LineType { get; set; } = "Principal";
        public string? Period { get; set; }
        public decimal? DailyRate { get; set; }
        public int? DaysInPeriod { get; set; }
        public decimal? BaseAmount { get; set; }
        public string? ImputationType { get; set; }
    }
}
