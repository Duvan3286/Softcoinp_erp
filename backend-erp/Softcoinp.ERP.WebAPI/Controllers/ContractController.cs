using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/contracts")]
public class ContractController : BaseController
{
    private readonly ContractService _contractService;
    private readonly RetentionService _retentionService;

    public ContractController(ContractService contractService, RetentionService retentionService)
    {
        _contractService = contractService;
        _retentionService = retentionService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
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
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
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
            var contract = await _contractService.UpdateContractAsync(tenantId, userId, id, request);
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

    [HttpPost("{contractId:guid}/policies")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ContractPolicyDto>> AddContractPolicy(Guid contractId, [FromBody] CreateContractPolicyRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var policy = await _contractService.AddContractPolicyAsync(tenantId, userId, contractId, request);
            return Ok(policy);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("alerts/active")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
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

    [HttpGet("retention-configurations")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<RetentionConfigurationDto>>> GetRetentionConfigurations()
    {
        var tenantId = GetTenantId();
        var configs = await _retentionService.GetRetentionConfigurationsAsync(tenantId);
        return Ok(configs);
    }

    [HttpPost("retention-configurations")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<RetentionConfigurationDto>> CreateRetentionConfiguration(
        [FromBody] CreateRetentionConfigurationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var config = await _retentionService.CreateRetentionConfigurationAsync(tenantId, userId, request);
            return Ok(config);
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

    [HttpPut("retention-configurations/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<RetentionConfigurationDto>> UpdateRetentionConfiguration(
        Guid id, [FromBody] UpdateRetentionConfigurationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var config = await _retentionService.UpdateRetentionConfigurationAsync(tenantId, userId, id, request);
            return Ok(config);
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

    [HttpGet("approval-thresholds")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<ApprovalThresholdDto>>> GetApprovalThresholds()
    {
        var tenantId = GetTenantId();
        var thresholds = await _retentionService.GetApprovalThresholdsAsync(tenantId);
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
            var threshold = await _retentionService.CreateApprovalThresholdAsync(tenantId, userId, request);
            return Ok(threshold);
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

    [HttpPut("approval-thresholds/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApprovalThresholdDto>> UpdateApprovalThreshold(
        Guid id, [FromBody] UpdateApprovalThresholdRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var threshold = await _retentionService.UpdateApprovalThresholdAsync(tenantId, userId, id, request);
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

    [HttpPost("calculate-retentions")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public ActionResult<RetentionCalculationDto> CalculateRetentions(
        [FromQuery] string serviceType, [FromQuery] decimal subtotal)
    {
        var tenantId = GetTenantId();

        try
        {
            var result = _retentionService.CalculateRetentions(tenantId, serviceType, subtotal);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
