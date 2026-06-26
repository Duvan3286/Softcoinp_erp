using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize]
public class FeesAndPortfolioController : BaseController
{
    private readonly BillingEngineService _billingEngine;
    private readonly LateInterestService _lateInterestService;
    private readonly PaymentService _paymentService;
    private readonly PaymentAgreementService _agreementService;
    private readonly StatementService _statementService;
    private readonly AccountingIntegrationService _accountingIntegration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeesAndPortfolioController> _logger;

    public FeesAndPortfolioController(
        BillingEngineService billingEngine,
        LateInterestService lateInterestService,
        PaymentService paymentService,
        PaymentAgreementService agreementService,
        StatementService statementService,
        AccountingIntegrationService accountingIntegration,
        ApplicationDbContext context,
        ILogger<FeesAndPortfolioController> logger)
    {
        _billingEngine = billingEngine;
        _lateInterestService = lateInterestService;
        _paymentService = paymentService;
        _agreementService = agreementService;
        _statementService = statementService;
        _accountingIntegration = accountingIntegration;
        _context = context;
        _logger = logger;
    }

    [HttpGet("checklist")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetBillingChecklist([FromQuery] string period)
    {
        var tenantId = GetTenantId();
        var checklist = await _billingEngine.GetBillingChecklistAsync(tenantId, period);
        return Ok(checklist);
    }

    [HttpPost("execute")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ExecuteMonthlyBilling([FromBody] ExecuteBillingRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var billingPeriod = await _billingEngine.ExecuteMonthlyBillingAsync(
                tenantId,
                request.Period,
                request.CutoffDate,
                request.PaymentDueDate,
                userId);

            return Ok(new
            {
                id = billingPeriod.Id,
                period = billingPeriod.Period,
                status = billingPeriod.Status.ToString(),
                totalBilled = billingPeriod.MonthlyBudgetTotal,
                roundingAdjustment = billingPeriod.RoundingAdjustment,
                executedAt = billingPeriod.ExecutedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetBillingPeriods()
    {
        var tenantId = GetTenantId();
        var periods = await _billingEngine.GetBillingPeriodsAsync(tenantId);
        return Ok(periods);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetBillingPeriodDetail(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var detail = await _billingEngine.GetBillingPeriodDetailAsync(tenantId, id);
            return Ok(detail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/notes")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateBillingNotes(Guid id, [FromBody] string notes)
    {
        var tenantId = GetTenantId();

        var billingPeriod = await _context.BillingPeriods
            .FirstOrDefaultAsync(bp => bp.Id == id && bp.TenantId == tenantId);

        if (billingPeriod == null)
        {
            return NotFound("No se encontró el período de liquidación.");
        }

        billingPeriod.Notes = notes;
        await _context.SaveChangesAsync();

        return Ok(new { id = billingPeriod.Id, notes = billingPeriod.Notes });
    }

    [HttpGet("portfolio-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetPortfolioSummary()
    {
        var tenantId = GetTenantId();

        var unitFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId)
            .ToListAsync();

        var totalBilled = unitFees.Sum(uf => uf.FeeValue);
        var totalCollected = unitFees.Sum(uf => uf.PaidAmount);
        var totalOutstanding = unitFees.Sum(uf => uf.BalanceAmount);
        var collectionRate = totalBilled > 0 ? Math.Round(totalCollected / totalBilled * 100m, 2) : 100m;

        var unitsWithBalance = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.BalanceAmount > 0)
            .Select(uf => uf.UnitId)
            .Distinct()
            .CountAsync();

        var totalUnits = await _context.Units
            .CountAsync(u => u.TenantId == tenantId
                          && (u.Status == UnitStatus.ActiveOccupied
                           || u.Status == UnitStatus.ActiveUnoccupied
                           || u.Status == UnitStatus.DeliveryProcess));

        var now = DateTime.UtcNow;

        var agingData = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.BalanceAmount > 0)
            .GroupBy(uf => uf.DueDate < now.AddMonths(-6) ? "6+ meses"
                        : uf.DueDate < now.AddMonths(-3) ? "4-6 meses"
                        : uf.DueDate < now.AddMonths(-1) ? "1-3 meses"
                        : "Corriente")
            .Select(g => new { Bucket = g.Key, Total = g.Sum(uf => uf.BalanceAmount) })
            .ToListAsync();

        var unitCountByBucket = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.BalanceAmount > 0)
            .GroupBy(uf => uf.DueDate < now.AddMonths(-6) ? "6+ meses"
                        : uf.DueDate < now.AddMonths(-3) ? "4-6 meses"
                        : uf.DueDate < now.AddMonths(-1) ? "1-3 meses"
                        : "Corriente")
            .Select(g => new { Bucket = g.Key, Count = g.Select(uf => uf.UnitId).Distinct().Count() })
            .ToListAsync();

        var agingBuckets = agingData.Select(a => new AgingBucketDto
        {
            Bucket = a.Bucket,
            UnitCount = unitCountByBucket.FirstOrDefault(u => u.Bucket == a.Bucket)?.Count ?? 0,
            TotalDebt = a.Total
        }).OrderBy(a => a.Bucket).ToList();

        var summary = new PortfolioSummaryDto
        {
            TotalBilled = totalBilled,
            TotalCollected = totalCollected,
            TotalOutstanding = totalOutstanding,
            CollectionRate = collectionRate,
            UnitsWithDebt = unitsWithBalance,
            TotalUnits = totalUnits,
            AgingBuckets = agingBuckets
        };

        return Ok(summary);
    }

    [HttpGet("interest-rate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetInterestRate()
    {
        var tenantId = GetTenantId();
        var monthlyRate = await _lateInterestService.GetMonthlyRateAsync(tenantId);
        var dailyRate = _lateInterestService.GetDailyRate(monthlyRate);

        var config = await _context.TenantConfigurations.FirstOrDefaultAsync();

        return Ok(new LateInterestRateConfigDto
        {
            MonthlyRate = monthlyRate,
            MaxLegalRate = config?.MaxLegalInterestRate ?? 0m,
            DailyRate = dailyRate
        });
    }

    [HttpGet("units/{unitId}/interest-preview")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> PreviewUnitInterest(Guid unitId, [FromQuery] DateTime? asOfDate)
    {
        var tenantId = GetTenantId();
        var date = asOfDate ?? DateTime.UtcNow;
        var interests = await _lateInterestService.PreviewUnitInterestAsync(tenantId, unitId, date);
        return Ok(new
        {
            unitId,
            asOfDate = date,
            totalInterest = Math.Round(interests.Sum(i => i.CalculatedInterest), 2),
            items = interests
        });
    }

    [HttpPost("interest/capitalize")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CapitalizeInterest([FromBody] CapitalizeInterestRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var interests = await _lateInterestService.CapitalizeInterestAsync(
                tenantId, request.SourceType, request.SourceId, request.Period, userId);

            return Ok(new { count = interests.Count, total = interests.Sum(i => i.CalculatedAmount) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("interest/capitalize-all")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CapitalizeAllInterest([FromBody] CapitalizeAllInterestRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var interests = await _lateInterestService.CapitalizeAllOverdueInterestAsync(
            tenantId, request.Period, userId);
        return Ok(new { count = interests.Count, total = interests.Sum(i => i.CalculatedAmount) });
    }

    [HttpGet("interest/capitalized")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetCapitalizedInterests([FromQuery] Guid? unitId)
    {
        var tenantId = GetTenantId();
        var interests = await _lateInterestService.GetCapitalizedInterestsAsync(tenantId, unitId);
        return Ok(interests);
    }

    [HttpGet("units/{unitId}/debt")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitDebtSummary(Guid unitId)
    {
        var tenantId = GetTenantId();

        try
        {
            var summary = await _paymentService.GetUnitDebtSummaryAsync(tenantId, unitId);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("payment/preview")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> PreviewPayment([FromBody] RegisterPaymentRequestDto request)
    {
        var tenantId = GetTenantId();
        var preview = await _paymentService.PreviewPaymentAllocationAsync(
            tenantId, request.UnitId, request.Amount);
        return Ok(preview);
    }

    [HttpPost("payment/register")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> RegisterPayment([FromBody] RegisterPaymentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var payment = await _paymentService.RegisterPaymentAsync(tenantId, request, userId);

            return Ok(new
            {
                id = payment.Id,
                unitId = payment.UnitId,
                amount = payment.Amount,
                advanceAmount = payment.AdvanceAmount,
                paymentDate = payment.PaymentDate
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("units/{unitId}/payments")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitPayments(Guid unitId)
    {
        var tenantId = GetTenantId();
        var payments = await _paymentService.GetUnitPaymentsAsync(tenantId, unitId);
        return Ok(payments);
    }

    [HttpGet("payments/{paymentId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetPaymentDetail(Guid paymentId)
    {
        var tenantId = GetTenantId();

        try
        {
            var detail = await _paymentService.GetPaymentDetailAsync(tenantId, paymentId);
            return Ok(detail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("agreements/simulate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public IActionResult SimulateAgreement([FromBody] CreatePaymentAgreementRequestDto request)
    {
        var simulation = _agreementService.SimulateAgreement(
            request.TotalDebtIncluded,
            request.NumberOfInstallments,
            request.InterestForgivenessPercentage,
            request.StartDate);

        return Ok(simulation);
    }

    [HttpPost("agreements")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateAgreement([FromBody] CreatePaymentAgreementRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var agreement = await _agreementService.CreateAgreementAsync(tenantId, request, userId);
            return Ok(new
            {
                id = agreement.Id,
                unitId = agreement.UnitId,
                status = agreement.Status.ToString(),
                installments = agreement.NumberOfInstallments,
                installmentAmount = agreement.InstallmentAmount
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("agreements")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetAgreements()
    {
        var tenantId = GetTenantId();
        var agreements = await _agreementService.GetActiveAgreementsAsync(tenantId);
        return Ok(agreements);
    }

    [HttpGet("agreements/{agreementId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetAgreementDetail(Guid agreementId)
    {
        var tenantId = GetTenantId();

        try
        {
            var detail = await _agreementService.GetAgreementDetailAsync(tenantId, agreementId);
            return Ok(detail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("agreements/{agreementId}/pay")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> PayAgreementInstallment(Guid agreementId, [FromBody] PayAgreementRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            await _agreementService.ApplyPaymentToAgreementAsync(tenantId, agreementId, request.Amount);
            return Ok(new { message = "Pago aplicado al acuerdo exitosamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("statement")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitStatement([FromBody] StatementRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            var statement = await _statementService.GetUnitStatementAsync(
                tenantId, request.UnitId, request.StartDate, request.EndDate);
            return Ok(statement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("clearance-certificate/issue")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> IssueClearanceCertificate(
        [FromBody] IssueClearanceCertificateRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var cert = await _statementService.IssueClearanceCertificateAsync(
                tenantId, request.UnitId, request.ValidityDays, userId);
            return Ok(new
            {
                id = cert.Id,
                certificateNumber = cert.CertificateNumber,
                issueDate = cert.IssueDate,
                expirationDate = cert.ExpirationDate,
                status = cert.Status.ToString()
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("units/{unitId}/clearance-certificates")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitCertificates(Guid unitId)
    {
        var tenantId = GetTenantId();
        var certificates = await _statementService.GetUnitCertificatesAsync(tenantId, unitId);
        return Ok(certificates);
    }

    [HttpGet("clearance-certificates/{certificateId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetCertificateDetail(Guid certificateId)
    {
        var tenantId = GetTenantId();

        try
        {
            var detail = await _statementService.GetCertificateDetailAsync(tenantId, certificateId);
            return Ok(detail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("clearance-certificates/{certificateId}/revoke")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RevokeCertificate(Guid certificateId)
    {
        var tenantId = GetTenantId();

        try
        {
            await _statementService.RevokeCertificateAsync(tenantId, certificateId);
            return Ok(new { message = "Paz y salvo revocado exitosamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("extraordinary-fees")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateExtraordinaryFee(
        [FromBody] CreateExtraordinaryFeeRequestDto request)
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("El nombre de la cuota extraordinaria es obligatorio.");
        if (request.TotalAmount <= 0)
            return BadRequest("El monto total debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(request.MeetingActNumber))
            return BadRequest("El número del acta de asamblea es obligatorio para cuotas extraordinarias.");

        if (!Enum.TryParse<DistributionType>(request.DistributionType, true, out var distributionType))
            return BadRequest("Tipo de distribución inválido. Use: AllByCoefficient o SpecificGroup.");

        var unitsQuery = _context.Units
            .Where(u => u.TenantId == tenantId
                && (u.Status == UnitStatus.ActiveOccupied || u.Status == UnitStatus.ActiveUnoccupied));

        if (distributionType == DistributionType.SpecificGroup && request.UnitIds?.Count > 0)
        {
            unitsQuery = unitsQuery.Where(u => request.UnitIds.Contains(u.Id));
        }

        var units = await unitsQuery.ToListAsync();

        if (units.Count == 0)
            return BadRequest("No hay unidades activas para distribuir la cuota.");

        var totalCoefficients = units.Sum(u => u.CoproprietyCoefficient);

        var fee = new ExtraordinaryFee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Notes,
            MeetingActNumber = request.MeetingActNumber,
            TotalAmount = request.TotalAmount,
            DistributionType = distributionType,
            NumberOfInstallments = request.NumberOfInstallments,
            StartPeriod = request.StartPeriod,
            ApprovalDate = DateTime.UtcNow,
            Status = ExtraordinaryFeeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.ExtraordinaryFees.Add(fee);

            var distributions = new List<ExtraordinaryFeeDistribution>();
            foreach (var unit in units)
            {
                decimal unitAmount;
                if (distributionType == DistributionType.AllByCoefficient && totalCoefficients > 0)
                {
                    unitAmount = Math.Round(request.TotalAmount * unit.CoproprietyCoefficient / totalCoefficients, 2);
                }
                else
                {
                    unitAmount = Math.Round(request.TotalAmount / units.Count, 2);
                }

                for (int i = 1; i <= request.NumberOfInstallments; i++)
                {
                    var installDueDate = request.DueDate.AddMonths(i - 1);
                    var installmentAmount = Math.Round(unitAmount / request.NumberOfInstallments, 2);
                    distributions.Add(new ExtraordinaryFeeDistribution
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        ExtraordinaryFeeId = fee.Id,
                        UnitId = unit.Id,
                        Amount = installmentAmount,
                        InstallmentNumber = i,
                        DueDate = installDueDate,
                        Status = FeeStatus.Pending,
                        BalanceAmount = installmentAmount
                    });
                }
            }

            _context.ExtraordinaryFeeDistributions.AddRange(distributions);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            try
            {
                await _accountingIntegration.RecordExtraordinaryFeeAsync(
                    tenantId, fee.Id, fee.TotalAmount,
                    $"Cuota extraordinaria: {fee.Name} ({fee.StartPeriod})", GetUserId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar asiento contable de cuota extraordinaria {FeeId} para tenant {TenantId}", fee.Id, tenantId);
            }

            var firstUnitAmount = distributions.Count > 0 ? distributions[0].Amount : 0m;
            return Ok(new { id = fee.Id, name = fee.Name, amountPerUnit = firstUnitAmount, distributionsCount = distributions.Count });
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpGet("extraordinary-fees")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetExtraordinaryFees()
    {
        var tenantId = GetTenantId();

        var fees = await _context.ExtraordinaryFees
            .Include(f => f.Distributions)
            .Where(f => f.TenantId == tenantId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var feeDtos = fees.Select(f => new ExtraordinaryFeeDto
        {
            Id = f.Id,
            Name = f.Name,
            TotalAmount = f.TotalAmount,
            DistributionType = f.DistributionType.ToString(),
            DueDate = f.Distributions.OrderBy(d => d.DueDate).Select(d => d.DueDate).FirstOrDefault(),
            NumberOfInstallments = f.NumberOfInstallments,
            AmountPerUnit = f.TotalAmount / Math.Max(1, f.Distributions.Select(d => d.UnitId).Distinct().Count()),
            Status = f.Status.ToString(),
            CreatedAt = f.CreatedAt,
            TotalCollected = f.Distributions.Where(d => d.Status == FeeStatus.FullyPaid).Sum(d => d.PaidAmount),
            TotalOutstanding = f.Distributions.Where(d => d.Status != FeeStatus.FullyPaid).Sum(d => d.BalanceAmount),
            UnitsCount = f.Distributions.Select(d => d.UnitId).Distinct().Count()
        }).ToList();

        return Ok(feeDtos);
    }

    [HttpGet("extraordinary-fees/{feeId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetExtraordinaryFeeDetail(Guid feeId)
    {
        var tenantId = GetTenantId();

        var fee = await _context.ExtraordinaryFees
            .Include(f => f.Distributions)
                .ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(f => f.Id == feeId && f.TenantId == tenantId);

        if (fee == null)
            return NotFound("Cuota extraordinaria no encontrada.");

        var dto = new ExtraordinaryFeeDetailDto
        {
            Id = fee.Id,
            Name = fee.Name,
            TotalAmount = fee.TotalAmount,
            DistributionType = fee.DistributionType.ToString(),
            DueDate = fee.Distributions.OrderBy(d => d.DueDate).Select(d => d.DueDate).FirstOrDefault(),
            NumberOfInstallments = fee.NumberOfInstallments,
            AmountPerUnit = fee.TotalAmount / Math.Max(1, fee.Distributions.Select(d => d.UnitId).Distinct().Count()),
            Status = fee.Status.ToString(),
            Notes = fee.Description,
            CreatedAt = fee.CreatedAt,
            Distributions = fee.Distributions.Select(d => new ExtraordinaryFeeDistributionDto
            {
                Id = d.Id,
                UnitId = d.UnitId,
                UnitIdentifier = d.Unit?.Identifier ?? string.Empty,
                Amount = d.Amount,
                InstallmentNumber = d.InstallmentNumber,
                DueDate = d.DueDate,
                Status = d.Status.ToString(),
                PaidAmount = d.PaidAmount,
                BalanceAmount = d.BalanceAmount
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPut("extraordinary-fees/{feeId}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateExtraordinaryFeeStatus(
        Guid feeId, [FromBody] UpdateExtraordinaryFeeStatusRequestDto request)
    {
        var tenantId = GetTenantId();

        var fee = await _context.ExtraordinaryFees
            .FirstOrDefaultAsync(f => f.Id == feeId && f.TenantId == tenantId);

        if (fee == null)
            return NotFound("Cuota extraordinaria no encontrada.");

        if (!Enum.TryParse<ExtraordinaryFeeStatus>(request.Status, true, out var newStatus))
            return BadRequest("Estado inválido.");

        fee.Status = newStatus;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Estado actualizado exitosamente.", status = newStatus.ToString() });
    }

    [HttpPost("individual-charges")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateIndividualCharge(
        [FromBody] CreateIndividualChargeRequestDto request)
    {
        var tenantId = GetTenantId();

        if (request.Amount <= 0)
            return BadRequest("El monto debe ser mayor a cero.");

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == request.UnitId && u.TenantId == tenantId);

        if (unit == null)
            return NotFound("Unidad no encontrada.");

        if (!Enum.TryParse<ChargeType>(request.ChargeType, true, out var chargeType))
            return BadRequest("Tipo de cobro inválido. Use: Fine, Damage, ParkingFee u Other.");

        if (chargeType == ChargeType.Fine && string.IsNullOrWhiteSpace(request.ReferenceActNumber))
            return BadRequest("El número de acta del Consejo de Administración es obligatorio para registrar multas.");

        var charge = new IndividualCharge
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = request.UnitId,
            ChargeType = chargeType,
            Concept = request.Concept,
            Amount = request.Amount,
            BalanceAmount = request.Amount,
            Description = request.Description,
            ChargeDate = request.ChargeDate,
            ReferenceActNumber = request.ReferenceActNumber,
            Status = IndividualChargeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.IndividualCharges.Add(charge);
        await _context.SaveChangesAsync();

        try
        {
            await _accountingIntegration.RecordIndividualChargeAsync(
                tenantId, charge.Id, charge.Amount,
                $"Cargo individual: {charge.Concept} - Unidad {unit.Identifier}", GetUserId());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar asiento contable de cargo individual {ChargeId} para tenant {TenantId}", charge.Id, tenantId);
        }

        return Ok(new
        {
            id = charge.Id,
            unitId = charge.UnitId,
            amount = charge.Amount,
            description = charge.Description,
            status = charge.Status.ToString()
        });
    }

    [HttpGet("individual-charges")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetIndividualCharges(
        [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();

        var query = _context.IndividualCharges
            .Include(c => c.Unit)
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<IndividualChargeStatus>(status, true, out var chargeStatus))
        {
            query = query.Where(c => c.Status == chargeStatus);
        }

        var charges = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var chargeDtos = charges.Select(c => new IndividualChargeDto
        {
            Id = c.Id,
            UnitId = c.UnitId,
            UnitIdentifier = c.Unit?.Identifier ?? string.Empty,
            ChargeType = c.ChargeType.ToString(),
            Amount = c.Amount,
            BalanceAmount = c.BalanceAmount,
            Concept = c.Concept,
            Description = c.Description,
            ChargeDate = c.ChargeDate,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(chargeDtos);
    }

    [HttpGet("units/{unitId}/individual-charges")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitIndividualCharges(Guid unitId)
    {
        var tenantId = GetTenantId();

        var charges = await _context.IndividualCharges
            .Where(c => c.TenantId == tenantId && c.UnitId == unitId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var chargeDtos = charges.Select(c => new IndividualChargeDto
        {
            Id = c.Id,
            UnitId = c.UnitId,
            UnitIdentifier = c.Unit?.Identifier ?? string.Empty,
            ChargeType = c.ChargeType.ToString(),
            Amount = c.Amount,
            BalanceAmount = c.BalanceAmount,
            Concept = c.Concept,
            Description = c.Description,
            ChargeDate = c.ChargeDate,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(chargeDtos);
    }

    [HttpPut("individual-charges/{chargeId}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateIndividualChargeStatus(
        Guid chargeId, [FromBody] UpdateIndividualChargeStatusRequestDto request)
    {
        var tenantId = GetTenantId();

        var charge = await _context.IndividualCharges
            .FirstOrDefaultAsync(c => c.Id == chargeId && c.TenantId == tenantId);

        if (charge == null)
            return NotFound("Cobro individual no encontrado.");

        var allowedStatuses = new[] { "Pending", "Disputed", "Waived", "Paid" };
        if (!allowedStatuses.Contains(request.Status))
            return BadRequest("Estado inválido. Use: Pending, Disputed, Waived o Paid.");

        if (!Enum.TryParse<IndividualChargeStatus>(request.Status, true, out var newStatus))
            return BadRequest("Estado inválido.");

        charge.Status = newStatus;
        if (!string.IsNullOrEmpty(request.Notes))
        {
            charge.Description = $"{charge.Description} | {request.Notes}";
        }

        if (newStatus == IndividualChargeStatus.Paid)
        {
            charge.BalanceAmount = 0;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Estado actualizado exitosamente.", status = newStatus.ToString() });
    }

    [HttpGet("portfolio/collection-stages")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetCollectionStages()
    {
        var tenantId = GetTenantId();

        var units = await _context.Units
            .Where(u => u.TenantId == tenantId
                && (u.Status == UnitStatus.ActiveOccupied || u.Status == UnitStatus.ActiveUnoccupied))
            .ToListAsync();

        var unitIds = units.Select(u => u.Id).ToList();

        var unitFees = await _context.UnitFees
            .Where(f => unitIds.Contains(f.UnitId) && f.TenantId == tenantId)
            .ToListAsync();

        var extraordinaryDistributions = await _context.ExtraordinaryFeeDistributions
            .Where(d => unitIds.Contains(d.UnitId) && d.TenantId == tenantId)
            .ToListAsync();

        var individualCharges = await _context.IndividualCharges
            .Where(c => unitIds.Contains(c.UnitId) && c.TenantId == tenantId)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => unitIds.Contains(p.UnitId) && p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var agreements = await _context.PaymentAgreements
            .Include(a => a.Installments)
            .Where(a => unitIds.Contains(a.UnitId) && a.TenantId == tenantId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var nowDate = now.Date;

        var preventive = new List<CollectionStageUnitDto>();
        var preJudicial = new List<CollectionStageUnitDto>();
        var judicialList = new List<CollectionStageUnitDto>();
        var agreementList = new List<CollectionStageUnitDto>();

        foreach (var unit in units)
        {
            var overdueFees = unitFees
                .Where(f => f.UnitId == unit.Id && f.Status != FeeStatus.FullyPaid && f.DueDate < now)
                .ToList();

            var overdueExtraordinary = extraordinaryDistributions
                .Where(d => d.UnitId == unit.Id && d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .ToList();

            var overdueCharges = individualCharges
                .Where(c => c.UnitId == unit.Id && c.Status != IndividualChargeStatus.Paid && c.ChargeDate < now)
                .ToList();

            var totalDebt = overdueFees.Sum(f => f.BalanceAmount)
                + overdueExtraordinary.Sum(d => d.BalanceAmount)
                + overdueCharges.Sum(c => c.BalanceAmount);

            if (totalDebt <= 0)
                continue;

            var maxOverdueDays = overdueFees
                .Select(f => (nowDate - f.DueDate.Date).Days)
                .Concat(overdueExtraordinary.Select(d => (nowDate - d.DueDate.Date).Days))
                .Concat(overdueCharges.Select(c => (nowDate - c.ChargeDate.Date).Days))
                .DefaultIfEmpty(0)
                .Max();

            var lastPayment = payments
                .Where(p => p.UnitId == unit.Id)
                .FirstOrDefault();

            var stageDto = new CollectionStageUnitDto
            {
                UnitId = unit.Id,
                UnitIdentifier = unit.Identifier,
                TotalDebt = totalDebt,
                OverdueBalance = totalDebt,
                LateDays = maxOverdueDays,
                LastPaymentDate = lastPayment?.PaymentDate.ToString("yyyy-MM-dd") ?? "N/A"
            };

            var activeAgreement = agreements
                .FirstOrDefault(a => a.UnitId == unit.Id && a.Status == AgreementStatus.Active);

            if (activeAgreement != null)
            {
                stageDto.TotalDebt = activeAgreement.TotalDebtIncluded;
                stageDto.OverdueBalance = activeAgreement.Installments
                    .Where(i => i.Status == AgreementInstallmentStatus.Overdue)
                    .Sum(i => i.Amount - i.PaidAmount);
                agreementList.Add(stageDto);
            }
            else if (maxOverdueDays <= 60)
            {
                preventive.Add(stageDto);
            }
            else if (maxOverdueDays <= 120)
            {
                preJudicial.Add(stageDto);
            }
            else
            {
                judicialList.Add(stageDto);
            }
        }

        var result = new PortfolioCollectionStagesDto
        {
            Preventive = new CollectionStageDto
            {
                Stage = "Preventivo",
                UnitCount = preventive.Count,
                TotalDebt = preventive.Sum(u => u.TotalDebt),
                TotalOverdue = preventive.Sum(u => u.OverdueBalance),
                Units = preventive
            },
            PreJudicial = new CollectionStageDto
            {
                Stage = "Prejurídico",
                UnitCount = preJudicial.Count,
                TotalDebt = preJudicial.Sum(u => u.TotalDebt),
                TotalOverdue = preJudicial.Sum(u => u.OverdueBalance),
                Units = preJudicial
            },
            Judicial = new CollectionStageDto
            {
                Stage = "Jurídico",
                UnitCount = judicialList.Count,
                TotalDebt = judicialList.Sum(u => u.TotalDebt),
                TotalOverdue = judicialList.Sum(u => u.OverdueBalance),
                Units = judicialList
            },
            Agreement = new CollectionStageDto
            {
                Stage = "Acuerdo de Pago",
                UnitCount = agreementList.Count,
                TotalDebt = agreementList.Sum(u => u.TotalDebt),
                TotalOverdue = agreementList.Sum(u => u.OverdueBalance),
                Units = agreementList
            }
        };

        return Ok(result);
    }

    [HttpGet("units/{unitId}/portfolio-detail")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetUnitPortfolioDetail(Guid unitId)
    {
        var tenantId = GetTenantId();

        var unit = await _context.Units
            .Include(u => u.UnitType)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
            return NotFound("Unidad no encontrada.");

        var unitFees = await _context.UnitFees
            .Include(f => f.BillingPeriod)
            .Where(f => f.UnitId == unitId && f.TenantId == tenantId)
            .OrderBy(f => f.DueDate)
            .ToListAsync();

        var extraordinaryDistributions = await _context.ExtraordinaryFeeDistributions
            .Where(d => d.UnitId == unitId && d.TenantId == tenantId)
            .Include(d => d.ExtraordinaryFee)
            .OrderBy(d => d.DueDate)
            .ToListAsync();

        var individualCharges = await _context.IndividualCharges
            .Where(c => c.UnitId == unitId && c.TenantId == tenantId)
            .OrderByDescending(c => c.ChargeDate)
            .ToListAsync();

        var latestPayments = await _context.Payments
            .Where(p => p.UnitId == unitId && p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .ToListAsync();

        var advanceBalance = await _context.Payments
            .Where(p => p.UnitId == unitId && p.TenantId == tenantId && p.AdvanceAmount > 0)
            .SumAsync(p => p.AdvanceAmount);

        var now = DateTime.UtcNow;
        var outstandingBalance = unitFees.Sum(f => f.BalanceAmount)
            + extraordinaryDistributions.Sum(d => d.BalanceAmount)
            + individualCharges.Sum(c => c.BalanceAmount);

        var overdueBalance = unitFees
            .Where(f => f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Sum(f => f.BalanceAmount)
            + extraordinaryDistributions
                .Where(d => d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .Sum(d => d.BalanceAmount)
            + individualCharges
                .Where(c => c.Status != IndividualChargeStatus.Paid && c.ChargeDate < now)
                .Sum(c => c.BalanceAmount);

        var maxLateDays = unitFees
            .Where(f => f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Select(f => (now - f.DueDate).Days)
            .Concat(extraordinaryDistributions
                .Where(d => d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .Select(d => (now - d.DueDate).Days))
            .Concat(individualCharges
                .Where(c => c.Status != IndividualChargeStatus.Paid && c.ChargeDate < now)
                .Select(c => (now - c.ChargeDate).Days))
            .DefaultIfEmpty(0)
            .Max();

        var lateDays = Math.Max(0, maxLateDays);

        string collectionStage;
        if (lateDays <= 60)
            collectionStage = "Preventivo";
        else if (lateDays <= 120)
            collectionStage = "Prejurídico";
        else
            collectionStage = "Jurídico";

        var debtItems = new List<PortfolioDebtItemDto>();
        debtItems.AddRange(unitFees.Select(f => new PortfolioDebtItemDto
        {
            SourceType = "Cuota Ordinaria",
            Description = $"Período {f.BillingPeriod?.Period ?? "N/A"}",
            DueDate = f.DueDate,
            Amount = f.FeeValue,
            Balance = f.BalanceAmount,
            DaysOverdue = f.Status != FeeStatus.FullyPaid ? Math.Max(0, (now - f.DueDate).Days) : 0
        }));
        debtItems.AddRange(extraordinaryDistributions.Select(d => new PortfolioDebtItemDto
        {
            SourceType = "Cuota Extraordinaria",
            Description = $"{d.ExtraordinaryFee?.Name ?? "N/A"} - Cuota {d.InstallmentNumber}",
            DueDate = d.DueDate,
            Amount = d.Amount,
            Balance = d.BalanceAmount,
            DaysOverdue = d.Status != FeeStatus.FullyPaid ? Math.Max(0, (now - d.DueDate).Days) : 0
        }));
        debtItems.AddRange(individualCharges.Select(c => new PortfolioDebtItemDto
        {
            SourceType = c.ChargeType switch
            {
                ChargeType.Fine => "Multa",
                ChargeType.Damage => "Daño",
                ChargeType.ParkingFee => "Parqueadero",
                _ => "Otro"
            },
            Description = c.Description,
            DueDate = c.ChargeDate,
            Amount = c.Amount,
            Balance = c.BalanceAmount,
            DaysOverdue = c.Status != IndividualChargeStatus.Paid ? Math.Max(0, (now - c.ChargeDate).Days) : 0
        }));

        var monthlyRate = await _lateInterestService.GetMonthlyRateAsync(tenantId);
        var dailyRate = _lateInterestService.GetDailyRate(monthlyRate);

        var detail = new UnitPortfolioDetailDto
        {
            UnitId = unit.Id,
            UnitIdentifier = unit.Identifier,
            UnitTower = unit.TowerOrBlock,
            UnitType = unit.UnitType?.Name ?? string.Empty,
            OutstandingBalance = outstandingBalance,
            OverdueBalance = overdueBalance,
            AdvanceBalance = advanceBalance,
            AccruedInterest = unitFees
                .Where(f => f.Status != FeeStatus.FullyPaid && f.DueDate < now)
                .Sum(f => Math.Round(f.BalanceAmount * dailyRate * Math.Max(0, (now - f.DueDate).Days), 2)),
            LateDays = lateDays,
            CollectionStage = collectionStage,
            DebtItems = debtItems.OrderByDescending(d => d.DaysOverdue).ToList(),
            RecentPayments = latestPayments.Select(p => new RecentPaymentDto
            {
                PaymentId = p.Id,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString()
            }).ToList()
        };

        return Ok(detail);
    }
}
