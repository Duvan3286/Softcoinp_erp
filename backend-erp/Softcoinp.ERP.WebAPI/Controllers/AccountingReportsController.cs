using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/accounting-reports")]
[Authorize]
public class AccountingReportsController : ControllerBase
{
    private readonly AccountingReportService _reportService;

    public AccountingReportsController(AccountingReportService reportService)
    {
        _reportService = reportService;
    }

    private string GetTenantId()
    {
        return User.FindFirstValue("tenant_id") ?? string.Empty;
    }

    [HttpGet("trial-balance")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] Guid? periodId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetTrialBalanceAsync(tenantId, periodId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("general-ledger/{accountId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetGeneralLedger(
        Guid accountId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetGeneralLedgerAsync(tenantId, accountId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("income-statement")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] Guid? periodId)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetIncomeStatementAsync(tenantId, periodId);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] Guid? periodId)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetBalanceSheetAsync(tenantId, periodId);
        return Ok(result);
    }

    [HttpGet("income-statement/comparative")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetComparativeIncomeStatement([FromQuery] Guid currentPeriodId, [FromQuery] Guid previousPeriodId)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetComparativeIncomeStatementAsync(tenantId, currentPeriodId, previousPeriodId);
        return Ok(result);
    }

    [HttpGet("balance-sheet/comparative")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetComparativeBalanceSheet([FromQuery] Guid currentPeriodId, [FromQuery] Guid previousPeriodId)
    {
        var tenantId = GetTenantId();
        var result = await _reportService.GetComparativeBalanceSheetAsync(tenantId, currentPeriodId, previousPeriodId);
        return Ok(result);
    }
}
