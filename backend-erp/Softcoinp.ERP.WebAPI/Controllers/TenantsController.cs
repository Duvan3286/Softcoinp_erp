using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.Infrastructure.Services;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/v1/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private static readonly Regex SubdomainPattern = new("^[a-z0-9_-]{2,40}$", RegexOptions.Compiled);

    // Must match SubdomainTenantResolver's cache key exactly so toggling status
    // takes effect immediately instead of waiting out the resolver's TTL.
    private const string TenantCacheKeyPrefix = "Tenant_";

    private readonly MasterDbContext _context;
    private readonly DatabaseMigrationService _migrationService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public TenantsController(MasterDbContext context, DatabaseMigrationService migrationService, IConfiguration configuration, IMemoryCache cache)
    {
        _context = context;
        _migrationService = migrationService;
        _configuration = configuration;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await _context.Tenants.CountAsync();
        var tenants = await _context.Tenants
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = await Task.WhenAll(tenants.Select(BuildTenantDtoAsync));

        return Ok(new
        {
            items,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            page,
            pageSize
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var subdomain = dto.Subdomain.Trim().ToLowerInvariant();
        if (!SubdomainPattern.IsMatch(subdomain))
        {
            return BadRequest(new { message = "El subdominio solo puede contener minusculas, numeros, guiones y guiones bajos (2-40 caracteres)." });
        }

        var alreadyExists = await _context.Tenants.AnyAsync(t => t.Subdomain == subdomain);
        if (alreadyExists)
        {
            return Conflict(new { message = $"Ya existe un tenant con el subdominio '{subdomain}'." });
        }

        var masterConnectionString = _configuration.GetConnectionString("MasterConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__MasterConnection")
            ?? throw new InvalidOperationException("MasterConnection no esta configurada.");

        var connBuilder = new MySqlConnectionStringBuilder(masterConnectionString)
        {
            Database = $"erp_{subdomain}"
        };

        var tenant = new Tenant
        {
            Name = subdomain,
            Subdomain = subdomain,
            ConnectionString = connBuilder.ConnectionString,
            IsActive = true
        };

        _context.Tenants.Add(tenant);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        {
            return Conflict(new { message = $"Ya existe un tenant con el subdominio '{subdomain}'." });
        }

        var migrationStatus = await _migrationService.MigrateTenantAsync(tenant);
        var dto2 = await BuildTenantDtoAsync(tenant);

        return CreatedAtAction(nameof(GetAll), new { id = tenant.Id }, new
        {
            tenant = dto2,
            initialization = migrationStatus
        });
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null)
        {
            return NotFound();
        }

        tenant.IsActive = !tenant.IsActive;
        await _context.SaveChangesAsync();

        // Evict the resolver's cached copy so suspension/reactivation is effective
        // on the very next request instead of lingering for up to its TTL.
        _cache.Remove($"{TenantCacheKeyPrefix}{tenant.Subdomain}");

        return Ok(new ToggleTenantStatusResponse { Id = tenant.Id, IsActive = tenant.IsActive });
    }

    private static async Task<object> BuildTenantDtoAsync(Tenant tenant)
    {
        var metrics = await TryGetMetricsAsync(tenant);

        return new
        {
            id = tenant.Id,
            name = tenant.Name,
            subdomain = tenant.Subdomain,
            isActive = tenant.IsActive,
            createdAt = tenant.CreatedAt,
            connectionString = MaskPassword(tenant.ConnectionString),
            metrics
        };
    }

    private static string MaskPassword(string connectionString)
    {
        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password))
            {
                builder.Password = "****";
            }
            return builder.ConnectionString;
        }
        catch
        {
            return Regex.Replace(connectionString, "Password=[^;]*", "Password=****", RegexOptions.IgnoreCase);
        }
    }

    private static async Task<object?> TryGetMetricsAsync(Tenant tenant)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var connBuilder = new MySqlConnectionStringBuilder(tenant.ConnectionString);
            var dbName = connBuilder.Database;

            var stopwatch = Stopwatch.StartNew();
            await using var connection = new MySqlConnection(tenant.ConnectionString);
            await connection.OpenAsync(cts.Token);
            stopwatch.Stop();

            long tableCount = 0, rowCount = 0;
            double sizeMb = 0;
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(*), COALESCE(SUM(table_rows), 0), COALESCE(SUM(data_length + index_length), 0) / 1048576
                                     FROM information_schema.tables WHERE table_schema = @db";
                cmd.Parameters.AddWithValue("@db", dbName);
                await using var reader = await cmd.ExecuteReaderAsync(cts.Token);
                if (await reader.ReadAsync(cts.Token))
                {
                    tableCount = reader.GetInt64(0);
                    rowCount = reader.GetInt64(1);
                    sizeMb = reader.GetDouble(2);
                }
            }

            long activity24h = 0;
            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM erp_refresh_tokens WHERE CreatedAt >= UTC_TIMESTAMP() - INTERVAL 1 DAY";
                var result = await cmd.ExecuteScalarAsync(cts.Token);
                activity24h = Convert.ToInt64(result);
            }
            catch (MySqlException)
            {
                // Table not migrated yet (freshly created tenant) — no activity to report.
            }

            return new
            {
                databaseName = dbName,
                sizeMb,
                tableCount,
                rowCount,
                latencyMs = stopwatch.ElapsedMilliseconds,
                activity24h
            };
        }
        catch
        {
            return null;
        }
    }
}
