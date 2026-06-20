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
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/fixed-assets")]
[Authorize]
public class FixedAssetsController : BaseController
{
    private readonly FixedAssetService _fixedAssetService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FixedAssetsController> _logger;

    public FixedAssetsController(FixedAssetService fixedAssetService, ApplicationDbContext context, ILogger<FixedAssetsController> logger)
    {
        _fixedAssetService = fixedAssetService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetAssets([FromQuery] bool includeInactive = false)
    {
        var assets = await _fixedAssetService.GetAssetsAsync(GetTenantId(), includeInactive);
        return Ok(assets);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetAsset(Guid id)
    {
        var asset = await _fixedAssetService.GetAssetByIdAsync(GetTenantId(), id);
        if (asset == null) return NotFound(new { message = "Activo fijo no encontrado." });
        return Ok(asset);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateAsset([FromBody] FixedAsset asset)
    {
        var result = await _fixedAssetService.CreateAssetAsync(GetTenantId(), asset, GetUserId());
        return CreatedAtAction(nameof(GetAsset), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] FixedAsset asset)
    {
        asset.Id = id;
        try
        {
            var result = await _fixedAssetService.UpdateAssetAsync(GetTenantId(), asset, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/dispose")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> DisposeAsset(Guid id, [FromBody] DisposeAssetRequestDto dto)
    {
        try
        {
            var result = await _fixedAssetService.DisposeAssetAsync(GetTenantId(), id, dto.DisposalDate, dto.DisposalValue, dto.Reason, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("calculate-depreciation")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CalculateDepreciation([FromQuery] int year, [FromQuery] int month)
    {
        var count = await _fixedAssetService.CalculateMonthlyDepreciationAsync(GetTenantId(), year, month, GetUserId());
        return Ok(new { period = $"{year:D4}-{month:D2}", assetsDepreciated = count });
    }

    [HttpGet("{id}/depreciations")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetDepreciationHistory(Guid id)
    {
        var history = await _fixedAssetService.GetDepreciationHistoryAsync(GetTenantId(), id);
        return Ok(history);
    }
}

public class DisposeAssetRequestDto
{
    public DateTime DisposalDate { get; set; }
    public decimal? DisposalValue { get; set; }
    public string Reason { get; set; } = string.Empty;
}
