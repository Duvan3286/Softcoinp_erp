using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.Infrastructure.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/v1/admin/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly DatabaseMigrationService _migrationService;

    public MaintenanceController(DatabaseMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    /// <summary>
    /// Triggers database migrations for the Master DB and all active tenant databases.
    /// </summary>
    [HttpPost("migrate-all")]
    public async Task<IActionResult> MigrateAll()
    {
        var results = await _migrationService.MigrateAllAsync();
        
        if (results.Values.Any(v => v.StartsWith("Failed")))
        {
            return StatusCode(500, results);
        }

        return Ok(results);
    }
}
