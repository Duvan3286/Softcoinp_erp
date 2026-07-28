using System;
using System.Linq;
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
    private readonly PaymentService _paymentService;
    private readonly StatementService _statementService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeesAndPortfolioController> _logger;
    private readonly PortfolioAgingService _portfolioAgingService;
    private readonly IndicatorCacheService _indicatorCache;
    private readonly MonthlyInterestRateService _monthlyInterestRateService;
    private readonly InterestCalculationService _interestCalculationService;
    private readonly InterestReportService _interestReportService;

    public FeesAndPortfolioController(
        BillingEngineService billingEngine,
        PaymentService paymentService,
        StatementService statementService,
        ApplicationDbContext context,
        ILogger<FeesAndPortfolioController> logger,
        PortfolioAgingService portfolioAgingService,
        IndicatorCacheService indicatorCache,
        MonthlyInterestRateService monthlyInterestRateService,
        InterestCalculationService interestCalculationService,
        InterestReportService interestReportService)
    {
        _billingEngine = billingEngine;
        _paymentService = paymentService;
        _portfolioAgingService = portfolioAgingService;
        _statementService = statementService;
        _context = context;
        _logger = logger;
        _indicatorCache = indicatorCache;
        _monthlyInterestRateService = monthlyInterestRateService;
        _interestCalculationService = interestCalculationService;
        _interestReportService = interestReportService;
    }

    [HttpGet("checklist")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetBillingChecklist([FromQuery] string period)
    {
        var tenantId = GetTenantId();
        var checklist = await _billingEngine.GetBillingChecklistAsync(tenantId, period);
        return Ok(checklist);
    }

    [HttpPost("execute")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
                request.ExcludedUnits,
                userId);

            return Ok(new
            {
                id = billingPeriod.Id,
                period = billingPeriod.Period,
                status = billingPeriod.Status.ToString(),
                totalBilled = billingPeriod.TotalBilled,
                roundingAdjustment = billingPeriod.RoundingAdjustment,
                executedAt = billingPeriod.ExecutedAt
            });
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

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetBillingPeriods()
    {
        var tenantId = GetTenantId();
        var periods = await _billingEngine.GetBillingPeriodsAsync(tenantId);
        return Ok(periods);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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

    [HttpPost("adjustments")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateBillingAdjustmentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var adjustment = await _billingEngine.CreateAdjustmentAsync(tenantId, request, userId);
            return Ok(new
            {
                id = adjustment.Id,
                unitId = adjustment.UnitId,
                amount = adjustment.Amount,
                reason = adjustment.Reason,
                createdAt = adjustment.CreatedAt
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("units/{unitId}/adjustments")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUnitAdjustments(Guid unitId)
    {
        var tenantId = GetTenantId();
        var adjustments = await _billingEngine.GetUnitAdjustmentsAsync(tenantId, unitId);
        return Ok(adjustments);
    }

    [HttpGet("portfolio-summary")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPortfolioSummary()
    {
        var tenantId = GetTenantId();

        var unitFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId)
            .ToListAsync();

        var extraordinaryOutstanding = await _context.ExtraordinaryFeeDistributions
            .Where(d => d.TenantId == tenantId)
            .SumAsync(d => d.BalanceAmount);

        var chargesOutstanding = await _context.IndividualCharges
            .Where(c => c.TenantId == tenantId && !c.IsDisputed)
            .SumAsync(c => c.BalanceAmount);

        var interestOutstanding = await _context.AccruedInterests
            .Where(ai => ai.TenantId == tenantId && ai.Status == AccruedInterestStatus.Pending)
            .SumAsync(ai => ai.BalanceAmount);

        var totalBilled = unitFees.Sum(uf => uf.FeeValue);
        var totalCollected = unitFees.Sum(uf => uf.PaidAmount);
        var totalOutstanding = unitFees.Sum(uf => uf.BalanceAmount) + extraordinaryOutstanding + chargesOutstanding + interestOutstanding;
        var collectionRate = 100m;
        if (totalBilled > 0)
        {
            collectionRate = Math.Round(totalCollected / totalBilled * 100m, 2);
        }

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

        var overdueByUnit = await _portfolioAgingService.GetOverdueByUnitAsync(tenantId);

        var oneToThree = new AgingBucketDto { Bucket = "1-3 meses" };
        var fourToSix = new AgingBucketDto { Bucket = "4-6 meses" };
        var sixPlus = new AgingBucketDto { Bucket = "6+ meses" };

        foreach (var unit in overdueByUnit.Values)
        {
            var bucket = oneToThree;
            if (unit.MonthsOverdue > 6)
            {
                bucket = sixPlus;
            }
            else if (unit.MonthsOverdue > 3)
            {
                bucket = fourToSix;
            }

            bucket.UnitCount += 1;
            bucket.TotalDebt += unit.TotalDebt;
        }

        var overdueTotal = overdueByUnit.Values.Sum(u => u.TotalDebt);
        var currentBucket = new AgingBucketDto
        {
            Bucket = "Corriente",
            UnitCount = Math.Max(0, unitsWithBalance - overdueByUnit.Count),
            TotalDebt = Math.Max(0, totalOutstanding - overdueTotal)
        };

        var agingBuckets = new System.Collections.Generic.List<AgingBucketDto> { currentBucket, oneToThree, fourToSix, sixPlus };

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

    [HttpGet("units/{unitId}/debt")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> PreviewPayment([FromBody] RegisterPaymentRequestDto request)
    {
        var tenantId = GetTenantId();
        var preview = await _paymentService.PreviewPaymentAllocationAsync(
            tenantId, request.UnitId, request.Amount);
        return Ok(preview);
    }

    [HttpPost("payment/preview-manual")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> PreviewManualPayment([FromBody] ManualPaymentPreviewRequestDto request)
    {
        var tenantId = GetTenantId();
        var preview = await _paymentService.PreviewManualPaymentAsync(
            tenantId, request.UnitId, request.Allocations);
        return Ok(preview);
    }

    [HttpPost("payment/register")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUnitPayments(Guid unitId)
    {
        var tenantId = GetTenantId();
        var payments = await _paymentService.GetUnitPaymentsAsync(tenantId, unitId);
        return Ok(payments);
    }

    [HttpGet("payments/{paymentId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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

    [HttpPost("statement")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUnitCertificates(Guid unitId)
    {
        var tenantId = GetTenantId();
        var certificates = await _statementService.GetUnitCertificatesAsync(tenantId, unitId);
        return Ok(certificates);
    }

    [HttpGet("clearance-certificates/{certificateId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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

    [HttpGet("clearance-certificates/{certificateId}/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DownloadCertificatePdf(Guid certificateId)
    {
        var tenantId = GetTenantId();

        try
        {
            var pdfBytes = await _statementService.GenerateCertificatePdfAsync(tenantId, certificateId);
            return File(pdfBytes, "application/pdf", "paz-y-salvo-" + certificateId + ".pdf");
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateExtraordinaryFee(
        [FromBody] CreateExtraordinaryFeeRequestDto request)
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("El nombre de la cuota extraordinaria es obligatorio.");
        if (request.TotalAmount <= 0)
            return BadRequest("El monto total debe ser mayor a cero.");

        if (!Enum.TryParse<DistributionType>(request.DistributionType, true, out var distributionType))
            return BadRequest("Tipo de distribución inválido. Use: AllByCoefficient o SpecificGroup.");

        if (distributionType == DistributionType.SpecificGroup && (request.UnitIds == null || request.UnitIds.Count == 0))
            return BadRequest("Debe seleccionar al menos una unidad para una distribución específica.");

        var unitsQuery = _context.Units
            .Where(u => u.TenantId == tenantId
                && (u.Status == UnitStatus.ActiveOccupied || u.Status == UnitStatus.ActiveUnoccupied));

        if (distributionType == DistributionType.SpecificGroup)
        {
            unitsQuery = unitsQuery.Where(u => request.UnitIds!.Contains(u.Id));
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

            var distributions = new System.Collections.Generic.List<ExtraordinaryFeeDistribution>();
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

            await _indicatorCache.InvalidateAsync(tenantId, DashboardService.CollectionChartCacheKeyPrefix);
            await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);

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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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

        await _indicatorCache.InvalidateAsync(tenantId, DashboardService.CollectionChartCacheKeyPrefix);
        await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);

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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
            charge.Description = charge.Description + " | " + request.Notes;
        }

        if (newStatus == IndividualChargeStatus.Paid)
        {
            charge.BalanceAmount = 0;
        }

        await _context.SaveChangesAsync();

        await _indicatorCache.InvalidateAsync(tenantId, DashboardService.CollectionChartCacheKeyPrefix);
        await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);

        return Ok(new { message = "Estado actualizado exitosamente.", status = newStatus.ToString() });
    }

    [HttpGet("portfolio/collection-stages")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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

        var adjustments = await _context.BillingAdjustments
            .Where(a => unitIds.Contains(a.UnitId) && a.TenantId == tenantId)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => unitIds.Contains(p.UnitId) && p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var oneMonth = new System.Collections.Generic.List<CollectionStageUnitDto>();
        var twoMonths = new System.Collections.Generic.List<CollectionStageUnitDto>();
        var threeOrMoreMonths = new System.Collections.Generic.List<CollectionStageUnitDto>();

        foreach (var unit in units)
        {
            var overdueFees = unitFees
                .Where(f => f.UnitId == unit.Id && f.Status != FeeStatus.FullyPaid && f.DueDate < now)
                .ToList();

            var overdueExtraordinary = extraordinaryDistributions
                .Where(d => d.UnitId == unit.Id && d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .ToList();

            var overdueCharges = individualCharges
                .Where(c => c.UnitId == unit.Id && c.Status != IndividualChargeStatus.Paid && !c.IsDisputed && c.ChargeDate < now)
                .ToList();

            var positiveAdjustments = adjustments
                .Where(a => a.UnitId == unit.Id && a.Amount > 0)
                .ToList();

            var totalDebt = overdueFees.Sum(f => f.BalanceAmount)
                + overdueExtraordinary.Sum(d => d.BalanceAmount)
                + overdueCharges.Sum(c => c.BalanceAmount)
                + positiveAdjustments.Sum(a => a.Amount);

            if (totalDebt <= 0)
                continue;

            var referenceDates = overdueFees.Select(f => f.DueDate)
                .Concat(overdueExtraordinary.Select(d => d.DueDate))
                .Concat(overdueCharges.Select(c => c.ChargeDate))
                .Concat(positiveAdjustments.Select(a => a.CreatedAt))
                .ToList();

            var oldestReferenceDate = referenceDates.DefaultIfEmpty(now).Min();
            var monthsOverdue = CalculateMonthsOverdue(oldestReferenceDate, now);

            if (monthsOverdue <= 0)
                continue;

            var lastPayment = payments.FirstOrDefault(p => p.UnitId == unit.Id);

            var stageDto = new CollectionStageUnitDto
            {
                UnitId = unit.Id,
                UnitIdentifier = unit.Identifier,
                TotalDebt = totalDebt,
                MonthsOverdue = monthsOverdue,
                LastPaymentDate = lastPayment != null ? lastPayment.PaymentDate.ToString("yyyy-MM-dd") : "N/A"
            };

            if (monthsOverdue == 1)
            {
                oneMonth.Add(stageDto);
            }
            else if (monthsOverdue == 2)
            {
                twoMonths.Add(stageDto);
            }
            else
            {
                threeOrMoreMonths.Add(stageDto);
            }
        }

        var result = new PortfolioCollectionStagesDto
        {
            OneMonth = new CollectionStageDto
            {
                Stage = "1 mes",
                UnitCount = oneMonth.Count,
                TotalDebt = oneMonth.Sum(u => u.TotalDebt),
                Units = oneMonth
            },
            TwoMonths = new CollectionStageDto
            {
                Stage = "2 meses",
                UnitCount = twoMonths.Count,
                TotalDebt = twoMonths.Sum(u => u.TotalDebt),
                Units = twoMonths
            },
            ThreeOrMoreMonths = new CollectionStageDto
            {
                Stage = "3 o más meses",
                UnitCount = threeOrMoreMonths.Count,
                TotalDebt = threeOrMoreMonths.Sum(u => u.TotalDebt),
                Units = threeOrMoreMonths
            }
        };

        return Ok(result);
    }

    [HttpGet("units/{unitId}/portfolio-detail")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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

        var adjustments = await _context.BillingAdjustments
            .Where(a => a.UnitId == unitId && a.TenantId == tenantId)
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
        var adjustmentTotal = adjustments.Sum(a => a.Amount);

        var outstandingBalance = unitFees.Sum(f => f.BalanceAmount)
            + extraordinaryDistributions.Sum(d => d.BalanceAmount)
            + individualCharges.Where(c => !c.IsDisputed).Sum(c => c.BalanceAmount)
            + adjustmentTotal;

        var overdueBalance = unitFees
            .Where(f => f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Sum(f => f.BalanceAmount)
            + extraordinaryDistributions
                .Where(d => d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .Sum(d => d.BalanceAmount)
            + individualCharges
                .Where(c => c.Status != IndividualChargeStatus.Paid && !c.IsDisputed && c.ChargeDate < now)
                .Sum(c => c.BalanceAmount)
            + adjustments.Where(a => a.Amount > 0).Sum(a => a.Amount);

        var referenceDates = unitFees
            .Where(f => f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Select(f => f.DueDate)
            .Concat(extraordinaryDistributions
                .Where(d => d.Status != FeeStatus.FullyPaid && d.DueDate < now)
                .Select(d => d.DueDate))
            .Concat(individualCharges
                .Where(c => c.Status != IndividualChargeStatus.Paid && !c.IsDisputed && c.ChargeDate < now)
                .Select(c => c.ChargeDate))
            .ToList();

        var monthsOverdue = 0;
        if (referenceDates.Count > 0)
        {
            var oldestReferenceDate = referenceDates.Min();
            monthsOverdue = CalculateMonthsOverdue(oldestReferenceDate, now);
        }

        var debtItems = new System.Collections.Generic.List<PortfolioDebtItemDto>();
        debtItems.AddRange(unitFees.Select(f => new PortfolioDebtItemDto
        {
            SourceType = "Cuota Ordinaria",
            Description = "Período " + (f.BillingPeriod != null ? f.BillingPeriod.Period : "N/A"),
            DueDate = f.DueDate,
            Amount = f.FeeValue,
            Balance = f.BalanceAmount,
            DaysOverdue = f.Status != FeeStatus.FullyPaid ? Math.Max(0, (now - f.DueDate).Days) : 0
        }));
        debtItems.AddRange(extraordinaryDistributions.Select(d => new PortfolioDebtItemDto
        {
            SourceType = "Cuota Extraordinaria",
            Description = (d.ExtraordinaryFee != null ? d.ExtraordinaryFee.Name : "N/A") + " - Cuota " + d.InstallmentNumber,
            DueDate = d.DueDate,
            Amount = d.Amount,
            Balance = d.BalanceAmount,
            DaysOverdue = d.Status != FeeStatus.FullyPaid ? Math.Max(0, (now - d.DueDate).Days) : 0
        }));
        debtItems.AddRange(individualCharges.Select(c => new PortfolioDebtItemDto
        {
            SourceType = DescribeChargeType(c.ChargeType),
            Description = c.Description,
            DueDate = c.ChargeDate,
            Amount = c.Amount,
            Balance = c.BalanceAmount,
            DaysOverdue = c.Status != IndividualChargeStatus.Paid ? Math.Max(0, (now - c.ChargeDate).Days) : 0
        }));
        debtItems.AddRange(adjustments.Select(a => new PortfolioDebtItemDto
        {
            SourceType = "Ajuste",
            Description = a.Reason,
            DueDate = a.CreatedAt,
            Amount = a.Amount,
            Balance = a.Amount,
            DaysOverdue = 0
        }));

        var detail = new UnitPortfolioDetailDto
        {
            UnitId = unit.Id,
            UnitIdentifier = unit.Identifier,
            UnitTower = unit.TowerOrBlock,
            UnitType = unit.UnitType?.Name ?? string.Empty,
            OutstandingBalance = outstandingBalance,
            OverdueBalance = overdueBalance,
            AdvanceBalance = advanceBalance,
            MonthsOverdue = monthsOverdue,
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

    // ── Interest Rate Endpoints ────────────────────────────────────────────────

    [HttpGet("interest-rates")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestRates()
    {
        var tenantId = GetTenantId();
        var rates = await _monthlyInterestRateService.GetRatesAsync(tenantId);
        return Ok(rates);
    }

    [HttpGet("interest-rates/current")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetCurrentInterestRate()
    {
        var tenantId = GetTenantId();
        var now = DateTime.UtcNow;
        var rate = await _monthlyInterestRateService.GetRateForPeriodAsync(tenantId, now.Year, now.Month);
        if (rate == null)
        {
            return NotFound(new { message = $"No hay tasa registrada para el período {now.Year}-{now.Month:D2}." });
        }
        return Ok(rate);
    }

    [HttpGet("interest-rates/by-period")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestRateByPeriod([FromQuery] int year, [FromQuery] int month)
    {
        var tenantId = GetTenantId();
        var rate = await _monthlyInterestRateService.GetRateForPeriodAsync(tenantId, year, month);
        if (rate == null)
        {
            return NotFound(new { message = $"No hay tasa registrada para {year}-{month:D2}." });
        }
        return Ok(rate);
    }

    [HttpGet("interest-rates/{rateId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestRateById(Guid rateId)
    {
        var tenantId = GetTenantId();
        var rate = await _monthlyInterestRateService.GetRateByIdAsync(tenantId, rateId);
        if (rate == null)
        {
            return NotFound(new { message = "Tasa de interés no encontrada." });
        }
        return Ok(rate);
    }

    [HttpPost("interest-rates")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RegisterInterestRate([FromBody] RegisterInterestRateRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var result = await _monthlyInterestRateService.RegisterRateAsync(
            tenantId, request.Year, request.Month, request.CertifiedRate, request.AppliedRate, userId);

        if (result.HasErrors)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new
        {
            rate = result.Rate,
            isUpdate = result.IsUpdate,
            message = result.IsUpdate
                ? "Tasa actualizada exitosamente."
                : "Tasa registrada exitosamente."
        });
    }

    [HttpDelete("interest-rates/{rateId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteInterestRate(Guid rateId)
    {
        var tenantId = GetTenantId();
        var deleted = await _monthlyInterestRateService.DeleteRateAsync(tenantId, rateId);

        if (!deleted)
        {
            return BadRequest(new { message = "No se puede eliminar la tasa. Tiene intereses registrados o no existe." });
        }

        return Ok(new { message = "Tasa eliminada exitosamente." });
    }

    // ── Interest Configuration Endpoints ───────────────────────────────────────

    [HttpGet("interest-configuration")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestConfiguration()
    {
        var tenantId = GetTenantId();
        var config = await _context.LateInterestConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId);

        if (config == null)
        {
            return NotFound(new { message = "No hay configuración de intereses. Cree una primero." });
        }

        return Ok(new LateInterestConfigurationDto
        {
            Id = config.Id,
            InterestStartDays = config.InterestStartDays,
            ApplyToAllUnitsByDefault = config.ApplyToAllUnitsByDefault,
            AlertOnMissingMonthlyRate = config.AlertOnMissingMonthlyRate,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        });
    }

    [HttpPut("interest-configuration")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateInterestConfiguration([FromBody] UpdateInterestConfigurationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (request.InterestStartDays < 0)
        {
            return BadRequest(new { message = "Los días de gracia no pueden ser negativos." });
        }

        var config = await _context.LateInterestConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId);

        if (config == null)
        {
            config = new LateInterestConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InterestStartDays = request.InterestStartDays,
                ApplyToAllUnitsByDefault = request.ApplyToAllUnitsByDefault,
                AlertOnMissingMonthlyRate = request.AlertOnMissingMonthlyRate,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            _context.LateInterestConfigurations.Add(config);
        }
        else
        {
            config.InterestStartDays = request.InterestStartDays;
            config.ApplyToAllUnitsByDefault = request.ApplyToAllUnitsByDefault;
            config.AlertOnMissingMonthlyRate = request.AlertOnMissingMonthlyRate;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Configuración de intereses actualizada exitosamente." });
    }

    // ── Unit Interest Exception Endpoints ──────────────────────────────────────

    [HttpGet("interest-exceptions")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestExceptions()
    {
        var tenantId = GetTenantId();
        var exceptions = await _context.UnitInterestExceptions
            .Where(e => e.TenantId == tenantId)
            .Select(e => new UnitInterestExceptionDto
            {
                Id = e.Id,
                UnitId = e.UnitId,
                UnitIdentifier = e.Unit != null ? e.Unit.Identifier : string.Empty,
                InterestStartDays = e.InterestStartDays,
                Reason = e.Reason,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(exceptions);
    }

    [HttpGet("interest-exceptions/{unitId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestExceptionForUnit(Guid unitId)
    {
        var tenantId = GetTenantId();
        var exception = await _context.UnitInterestExceptions
            .Where(e => e.TenantId == tenantId && e.UnitId == unitId)
            .Select(e => new UnitInterestExceptionDto
            {
                Id = e.Id,
                UnitId = e.UnitId,
                UnitIdentifier = e.Unit != null ? e.Unit.Identifier : string.Empty,
                InterestStartDays = e.InterestStartDays,
                Reason = e.Reason,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (exception == null)
        {
            return NotFound(new { message = "La unidad no tiene excepción de intereses." });
        }

        return Ok(exception);
    }

    [HttpPost("interest-exceptions")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpsertInterestException([FromBody] UpsertInterestExceptionRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (request.InterestStartDays < 0)
        {
            return BadRequest(new { message = "Los días de gracia no pueden ser negativos." });
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { message = "Debe especificar una razón para la excepción." });
        }

        var unitExists = await _context.Units.AnyAsync(u => u.Id == request.UnitId && u.TenantId == tenantId);
        if (!unitExists)
        {
            return BadRequest(new { message = "La unidad no existe o no pertenece al tenant." });
        }

        var existing = await _context.UnitInterestExceptions
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.UnitId == request.UnitId);

        if (existing != null)
        {
            existing.InterestStartDays = request.InterestStartDays;
            existing.Reason = request.Reason;
        }
        else
        {
            existing = new UnitInterestException
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = request.UnitId,
                InterestStartDays = request.InterestStartDays,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            _context.UnitInterestExceptions.Add(existing);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Excepción de intereses guardada exitosamente." });
    }

    [HttpDelete("interest-exceptions/{exceptionId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteInterestException(Guid exceptionId)
    {
        var tenantId = GetTenantId();
        var exception = await _context.UnitInterestExceptions
            .FirstOrDefaultAsync(e => e.Id == exceptionId && e.TenantId == tenantId);

        if (exception == null)
        {
            return NotFound(new { message = "Excepción no encontrada." });
        }

        _context.UnitInterestExceptions.Remove(exception);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Excepción eliminada exitosamente." });
    }

    // ── Interest Calculation Endpoints ─────────────────────────────────────────

    [HttpPost("interest/calculate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CalculateInterests([FromBody] CalculateInterestRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var result = await _interestCalculationService.CalculateAndSaveInterestsAsync(
            tenantId, request.UnitId, userId);

        return Ok(new
        {
            createdCount = result.CreatedCount,
            updatedCount = result.UpdatedCount,
            hasMissingRates = result.HasMissingRates,
            alerts = result.Alerts,
            message = result.CreatedCount > 0 || result.UpdatedCount > 0
                ? $"Intereses calculados: {result.CreatedCount} creados, {result.UpdatedCount} actualizados."
                : "No se generaron nuevos intereses."
        });
    }

    [HttpGet("interest/check-missing-rates")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CheckMissingRates()
    {
        var tenantId = GetTenantId();
        var result = await _interestCalculationService.CheckMissingRatesAsync(tenantId);

        return Ok(new
        {
            currentPeriod = result.CurrentPeriod,
            hasRateForCurrentPeriod = result.HasRateForCurrentPeriod,
            alertEnabled = result.AlertEnabled,
            message = result.Message
        });
    }

    // ── Accrued Interests Endpoints ────────────────────────────────────────────

    [HttpGet("units/{unitId:guid}/accrued-interests")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAccruedInterests(Guid unitId)
    {
        var tenantId = GetTenantId();

        var interests = await _context.AccruedInterests
            .Where(ai => ai.TenantId == tenantId && ai.UnitId == unitId)
            .OrderBy(ai => ai.InterestStartDate)
            .Select(ai => new AccruedInterestDto
            {
                Id = ai.Id,
                UnitFeeId = ai.UnitFeeId,
                ExtraordinaryFeeDistributionId = ai.ExtraordinaryFeeDistributionId,
                IndividualChargeId = ai.IndividualChargeId,
                Period = ai.Period,
                DailyRate = ai.DailyRate,
                DaysInPeriod = ai.DaysInPeriod,
                BaseAmount = ai.BaseAmount,
                CalculatedAmount = ai.CalculatedAmount,
                BalanceAmount = ai.BalanceAmount,
                Status = ai.Status.ToString(),
                InterestStartDate = ai.InterestStartDate,
                InterestEndDate = ai.InterestEndDate,
                MonthlyInterestRateId = ai.MonthlyInterestRateId,
                CreatedAt = ai.CreatedAt
            })
            .ToListAsync();

        return Ok(interests);
    }

    // ── Interest Report Endpoints ──────────────────────────────────────────────

    [HttpGet("reports/interest")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetInterestReport(
        [FromQuery] Guid? unitId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var tenantId = GetTenantId();
        var report = await _interestReportService.GetReportDataAsync(tenantId, unitId, status, from, to);
        return Ok(report);
    }

    [HttpGet("reports/interest/export")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ExportInterestReport(
        [FromQuery] string format,
        [FromQuery] Guid? unitId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var tenantId = GetTenantId();

        if (format?.ToLower() == "excel")
        {
            var bytes = await _interestReportService.GenerateExcelAsync(tenantId, unitId, status, from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "reporte_intereses_mora.xlsx");
        }

        if (format?.ToLower() == "pdf")
        {
            var bytes = await _interestReportService.GeneratePdfAsync(tenantId, unitId, status, from, to);
            return File(bytes, "application/pdf", "reporte_intereses_mora.pdf");
        }

        return BadRequest(new { message = "Formato inválido. Use 'excel' o 'pdf'." });
    }

    private static string DescribeChargeType(ChargeType chargeType)
    {
        switch (chargeType)
        {
            case ChargeType.Fine:
                return "Multa";
            case ChargeType.Damage:
                return "Daño";
            case ChargeType.ParkingFee:
                return "Parqueadero";
            default:
                return "Otro";
        }
    }

    private static int CalculateMonthsOverdue(DateTime dueDate, DateTime now)
    {
        var months = ((now.Year - dueDate.Year) * 12) + (now.Month - dueDate.Month);
        if (now.Day < dueDate.Day)
        {
            months -= 1;
        }
        return Math.Max(0, months);
    }
}
