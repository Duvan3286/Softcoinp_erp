using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.WebAPI.Services;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : BaseController
{
    private readonly BudgetService _budgetService;
    private readonly BudgetExecutionService _executionService;
    private readonly BudgetMovementService _movementService;
    private readonly ContingencyFundService _contingencyFundService;
    private readonly ApplicationDbContext _context;

    public BudgetsController(
        BudgetService budgetService,
        BudgetExecutionService executionService,
        BudgetMovementService movementService,
        ContingencyFundService contingencyFundService,
        ApplicationDbContext context)
    {
        _budgetService = budgetService;
        _executionService = executionService;
        _movementService = movementService;
        _contingencyFundService = contingencyFundService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetBudgets([FromQuery] int? year)
    {
        var tenantId = GetTenantId();

        var query = _context.Budgets
            .Include(b => b.BudgetDetails)
            .Where(b => b.TenantId == tenantId);

        if (year.HasValue)
        {
            query = query.Where(b => b.FiscalPeriod == year.Value);
        }

        var budgets = await query
            .OrderByDescending(b => b.FiscalPeriod)
            .ThenBy(b => b.Status)
            .ToListAsync();

        var result = budgets.Select(b => new BudgetSummaryDto
        {
            Id = b.Id,
            FiscalPeriod = b.FiscalPeriod,
            ApprovalDate = b.ApprovalDate,
            MeetingActNumber = b.MeetingActNumber,
            Status = b.Status.ToString(),
            DetailsCount = b.BudgetDetails.Count,
            CreatedByUserId = b.CreatedByUserId
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var budget = await _budgetService.CreateBudgetAsync(
                tenantId,
                request.FiscalPeriod,
                request.MeetingActNumber,
                request.ApprovalDate,
                request.CopyFromPrevious,
                request.GlobalPercentageAdjustment,
                request.AccountAdjustments,
                request.ManualDetails,
                userId
            );

            return CreatedAtAction(nameof(GetExecutionReport), new { year = budget.FiscalPeriod }, new { id = budget.Id, fiscalPeriod = budget.FiscalPeriod, status = budget.Status.ToString() });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/details")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateDraftDetails(Guid id, [FromBody] List<CreateBudgetDetailRequestDto> details)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.UpdateDraftBudgetDetailsAsync(tenantId, id, details);
            return Ok(new { id = budget.Id, status = budget.Status.ToString(), detailsCount = budget.BudgetDetails.Count });
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

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ActivateBudget(Guid id, [FromBody] ActivateBudgetRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.ActivateBudgetAsync(tenantId, id, request.MeetingActNumber, request.ApprovalDate);
            return Ok(new { id = budget.Id, status = budget.Status.ToString(), meetingActNumber = budget.MeetingActNumber, approvalDate = budget.ApprovalDate });
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

    [HttpPost("{id}/close")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CloseBudget(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.CloseBudgetAsync(tenantId, id);
            return Ok(new { id = budget.Id, status = budget.Status.ToString() });
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

    [HttpGet("execution/{year}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetExecutionReport(int year)
    {
        var tenantId = GetTenantId();

        try
        {
            var report = await _executionService.GetBudgetExecutionReportAsync(tenantId, year);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("movements")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateMovement([FromBody] CreateBudgetMovementRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (!Enum.TryParse<BudgetMovementType>(request.MovementType, true, out var movementType))
        {
            return BadRequest("El tipo de movimiento especificado es inválido (Debe ser 'Addition' o 'Transfer').");
        }

        if (!Enum.TryParse<BudgetApprovalType>(request.ApprovalType, true, out var approvalType))
        {
            return BadRequest("El tipo de aprobación especificado es inválido (Debe ser 'Council' o 'Assembly').");
        }

        try
        {
            var movement = await _movementService.CreateMovementAsync(
                tenantId,
                request.BudgetId,
                movementType,
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                request.Justification,
                approvalType,
                request.MeetingActNumber,
                request.ApprovalDate,
                userId
            );

            return Ok(new { id = movement.Id, type = movement.MovementType.ToString(), amount = movement.Amount });
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

    [HttpGet("{budgetId}/movements")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetMovements(Guid budgetId)
    {
        var tenantId = GetTenantId();
        var list = await _movementService.GetMovementsByBudgetAsync(tenantId, budgetId);
        return Ok(list);
    }

    [HttpGet("contingency-fund")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetContingencyFund()
    {
        var tenantId = GetTenantId();
        var fund = await _contingencyFundService.GetContingencyFundStatusAsync(tenantId);
        return Ok(fund);
    }

    [HttpPost("contingency-fund/liquidate")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> LiquidateContingencyContribution([FromBody] LiquidateMonthlyContributionRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            var contribution = await _contingencyFundService.LiquidateMonthlyContributionAsync(tenantId, request.Year, request.Month);
            return Ok(new { id = contribution.Id, period = contribution.Period, amount = contribution.Amount, incomeBase = contribution.IncomeBase });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("contingency-fund/usage")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> RecordContingencyUsage([FromBody] RecordContingencyFundUsageRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var usage = await _contingencyFundService.RecordUsageAsync(
                tenantId,
                request.Amount,
                request.Justification,
                request.CouncilApprovalActNumber,
                request.ApprovalDate,
                userId
            );

            return Ok(new { id = usage.Id, amount = usage.Amount, act = usage.CouncilApprovalActNumber });
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
}
