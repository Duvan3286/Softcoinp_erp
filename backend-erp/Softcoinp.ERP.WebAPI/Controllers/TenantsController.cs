using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/v1/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private readonly MasterDbContext _context;

    public TenantsController(MasterDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Tenants.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var tenant = new Tenant
        {
            Name = dto.Name,
            Subdomain = dto.Subdomain,
            ConnectionString = dto.ConnectionString
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = tenant.Id }, tenant);
    }
}
