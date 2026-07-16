using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/contracts")]
public class ContractController : BaseController
{
    private readonly ContractService _contractService;

    public ContractController(ContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<ContractListDto>>> GetContracts(
        [FromQuery] string? status = null,
        [FromQuery] string? contractType = null,
        [FromQuery] Guid? providerId = null,
        [FromQuery] string? search = null)
    {
        var tenantId = GetTenantId();
        var contracts = await _contractService.GetContractsAsync(tenantId, status, contractType, providerId, search);
        return Ok(contracts);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractDetailDto>> GetContract(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var contract = await _contractService.GetContractByIdAsync(tenantId, id);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractDetailDto>> CreateContract([FromBody] CreateContractRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var contract = await _contractService.CreateContractAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractDetailDto>> UpdateContract(Guid id, [FromBody] UpdateContractRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var contract = await _contractService.UpdateContractDetailsAsync(tenantId, userId, id, request);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractDetailDto>> ChangeContractStatus(Guid id, [FromBody] ChangeContractStatusRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var contract = await _contractService.UpdateContractStatusAsync(tenantId, userId, id, request);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteContract(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            await _contractService.DeleteContractAsync(tenantId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("invoices")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractInvoiceDto>> CreateInvoice([FromBody] CreateProviderInvoiceRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var invoice = await _contractService.CreateInvoiceAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetContract), new { id = invoice.ContractId }, invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("invoices/{invoiceId:guid}/payments")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ProviderPaymentDto>> RegisterPayment(Guid invoiceId, [FromBody] CreateProviderPaymentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var payment = await _contractService.RegisterPaymentAsync(tenantId, userId, invoiceId, request);
            return Ok(new ProviderPaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod.ToString(),
                ReferenceNumber = payment.ReferenceNumber,
                Status = payment.Status.ToString()
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("invoices/{invoiceId:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CancelInvoice(Guid invoiceId)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            await _contractService.CancelInvoiceAsync(tenantId, userId, invoiceId);
            return Ok(new { message = "Factura anulada y ejecución presupuestal revertida correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("payments-pending")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<PendingPaymentDto>>> GetPendingPayments()
    {
        var tenantId = GetTenantId();
        var payments = await _contractService.GetPendingPaymentsAsync(tenantId);
        return Ok(payments);
    }

    [HttpGet("expiring-report")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<ContractExpirationReportDto>>> GetExpiringContractsReport(
        [FromQuery] int daysAhead = 90)
    {
        var tenantId = GetTenantId();
        var report = await _contractService.GetExpiringContractsReportAsync(tenantId, daysAhead);
        return Ok(report);
    }

    [HttpGet("alerts/active")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<ContractAlertDto>>> GetActiveAlerts()
    {
        var tenantId = GetTenantId();
        var alerts = await _contractService.GetActiveContractAlertsAsync(tenantId);
        return Ok(alerts);
    }

    [HttpPost("alerts/{alertId:guid}/resolve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ResolveAlert(Guid alertId)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            await _contractService.ResolveContractAlertAsync(tenantId, userId, alertId);
            return Ok(new { message = "Alerta resuelta correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("approval-thresholds")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<ApprovalThresholdDto>>> GetApprovalThresholds()
    {
        var tenantId = GetTenantId();
        var thresholds = await _contractService.GetApprovalThresholdsAsync(tenantId);
        return Ok(thresholds);
    }

    [HttpPost("approval-thresholds")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApprovalThresholdDto>> CreateApprovalThreshold(
        [FromBody] CreateApprovalThresholdRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var threshold = await _contractService.CreateApprovalThresholdAsync(tenantId, userId, request);
            return Ok(threshold);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("approval-thresholds/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApprovalThresholdDto>> UpdateApprovalThreshold(
        Guid id, [FromBody] UpdateApprovalThresholdRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var threshold = await _contractService.UpdateApprovalThresholdAsync(tenantId, userId, id, request);
            return Ok(threshold);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
