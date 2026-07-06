using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : BaseController
{
    private readonly BudgetService _budgetService;
    private readonly ExecutionEngineService _executionService;
    private readonly ExpenseService _expenseService;
    private readonly ApplicationDbContext _context;

    public BudgetsController(
        BudgetService budgetService,
        ExecutionEngineService executionService,
        ExpenseService expenseService,
        ApplicationDbContext context)
    {
        _budgetService = budgetService;
        _executionService = executionService;
        _expenseService = expenseService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetBudgets([FromQuery] int? year)
    {
        var tenantId = GetTenantId();
        var budgets = await _budgetService.GetBudgetsAsync(tenantId, year);
        return Ok(budgets);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        var tenantId = GetTenantId();
        var budget = await _budgetService.GetBudgetDetailAsync(tenantId, id);
        if (budget == null)
        {
            return NotFound("Presupuesto no encontrado.");
        }
        return Ok(budget);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var budget = await _budgetService.CreateBudgetAsync(tenantId, request, userId);
            return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, budget);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateDraftBudget(Guid id, [FromBody] UpdateDraftBudgetRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.UpdateDraftBudgetAsync(tenantId, id, request.IncomeItems, request.ExpenseItems);
            return Ok(budget);
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

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ApproveBudget(Guid id, [FromBody] ApproveBudgetRequestDto request)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.ApproveBudgetAsync(tenantId, id, request);
            return Ok(budget);
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

    [HttpPost("{id}/generate-next")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GenerateNextPeriodBudget(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var budget = await _budgetService.GenerateNextPeriodBudgetAsync(tenantId, id);
            return Ok(budget);
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
    public async Task<IActionResult> GetExecutionDashboard(int year)
    {
        var tenantId = GetTenantId();

        try
        {
            var dashboard = await _executionService.GetExecutionDashboardAsync(tenantId, year);
            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("expenses")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> RecordExpense([FromBody] RecordExpenseRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var expense = await _expenseService.RecordExpenseAsync(tenantId, request, userId);
            return Ok(expense);
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

    [HttpGet("expenses")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] Guid? expenseItemId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var tenantId = GetTenantId();
        var expenses = await _expenseService.GetExpensesAsync(tenantId, expenseItemId, fromDate, toDate);
        return Ok(expenses);
    }

    [HttpGet("modifications/{budgetId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetModifications(Guid budgetId)
    {
        var tenantId = GetTenantId();
        var modifications = await _expenseService.GetModificationsAsync(tenantId, budgetId);
        return Ok(modifications);
    }

    [HttpPost("modifications")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateModification([FromBody] CreateModificationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var mod = await _expenseService.CreateModificationAsync(tenantId, request, userId);
            return Ok(mod);
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

    [HttpGet("contingency-fund")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetContingencyFundStatus()
    {
        var tenantId = GetTenantId();
        var status = await _executionService.GetContingencyFundStatusAsync(tenantId);
        return Ok(status);
    }

    [HttpPost("contingency-fund/usage")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> RecordContingencyFundUsage([FromBody] RecordContingencyFundUsageRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var usage = await _expenseService.RecordContingencyFundUsageAsync(tenantId, request, userId);
            return Ok(usage);
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
}

public class UpdateDraftBudgetRequestDto
{
    public System.Collections.Generic.List<CreateIncomeItemDto> IncomeItems { get; set; } = new();
    public System.Collections.Generic.List<CreateExpenseItemDto> ExpenseItems { get; set; } = new();
}
