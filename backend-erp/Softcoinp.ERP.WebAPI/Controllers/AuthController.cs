using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _db;
    private readonly MasterDbContext _masterDb;
    private readonly ILogger<AuthController> _logger;
    private readonly ITenantResolver _tenantResolver;

    private const int MaxLoginAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int MaxDailyLockouts = 3;
    private const int RefreshTokenDays = 7;

    public AuthController(
        UserManager<User> userManager,
        IConfiguration configuration,
        ApplicationDbContext db,
        MasterDbContext masterDb,
        ILogger<AuthController> logger,
        ITenantResolver tenantResolver)
    {
        _userManager = userManager;
        _configuration = configuration;
        _db = db;
        _masterDb = masterDb;
        _logger = logger;
        _tenantResolver = tenantResolver;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = GetClientIp();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenant = HttpContext.Items["Tenant"] as Tenant;

        _logger.LogInformation("Login attempt: {Email} | Tenant: {Tenant} | IP: {Ip}",
            request.Email, tenant?.Subdomain ?? "none", ip);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            await WriteAuditAsync(null, request.Email, tenant?.Id, AuditEventType.LoginFailed, ip, userAgent,
                JsonSerializer.Serialize(new { reason = "user_not_found" }));
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        if (user.Status == UserStatus.Suspended)
        {
            await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.LoginFailed, ip, userAgent,
                JsonSerializer.Serialize(new { reason = "account_suspended" }));
            return Unauthorized(new { message = "Tu cuenta ha sido suspendida. Contacta al administrador del conjunto." });
        }

        if (user.Status != UserStatus.Active)
        {
            await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.LoginFailed, ip, userAgent,
                JsonSerializer.Serialize(new { reason = "account_inactive" }));
            return Unauthorized(new { message = "Tu cuenta está inactiva. Contacta al administrador." });
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes);
            await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.LoginFailed, ip, userAgent,
                JsonSerializer.Serialize(new { reason = "temp_locked", remaining_minutes = remaining }));
            return Unauthorized(new { message = $"Cuenta bloqueada temporalmente. Intenta de nuevo en {remaining} minuto(s)." });
        }

        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordOk)
        {
            await HandleFailedAttemptAsync(user, tenant?.Id, ip, userAgent);
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.DailyLockoutCount = 0;
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var tenantRole = tenant != null
            ? await _db.UserTenantRoles
                .Where(r => r.UserId == user.Id && r.TenantId == tenant.Id.ToString() && r.IsActive)
                .FirstOrDefaultAsync()
            : null;

        var identityRoles = await _userManager.GetRolesAsync(user);
        var effectiveRole = tenantRole?.Role.ToString()
                            ?? identityRoles.FirstOrDefault()
                            ?? string.Empty;

        var (jwt, jwtExpiry) = GenerateJwtToken(user, effectiveRole, tenant?.Id);
        var (rawRefresh, hashedRefresh) = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TenantId = tenant?.Id.ToString() ?? "",
            TokenHash = hashedRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
            CreatedFromIp = ip
        };
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.LoginSuccess, ip, userAgent);

        SetAuthCookies(jwt, jwtExpiry, rawRefresh);

        return Ok(new
        {
            user = new
            {
                id = user.Id,
                name = user.FullName,
                email = user.Email,
                role = effectiveRole,
                tenantId = tenant?.Id,
                tenantName = tenant?.Name
            },
            token = jwt,
            tokenExpiry = jwtExpiry,
            refreshToken = rawRefresh
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request)
    {
        var ip = GetClientIp();
        var rawRefresh = request?.RefreshToken ?? Request.Cookies["refresh_token"] ?? "";
        if (string.IsNullOrEmpty(rawRefresh))
            return Unauthorized(new { message = "Token de sesión inválido o expirado." });
        var tokenHash = HashToken(rawRefresh);

        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        if (stored == null)
            return Unauthorized(new { message = "Token de sesión inválido o expirado." });

        if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
        {
            if (stored.ReplacedByTokenHash != null)
            {
                _logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all tokens.", stored.UserId);
                await RevokeAllUserTokensAsync(stored.UserId);
            }
            return Unauthorized(new { message = "Token de sesión inválido o expirado." });
        }

        var user = stored.User!;

        if (user.Status == UserStatus.Suspended || user.Status != UserStatus.Active)
        {
            await RevokeAllUserTokensAsync(user.Id);
            return Unauthorized(new { message = "El usuario ha sido suspendido o desactivado." });
        }

        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var identityRoles = await _userManager.GetRolesAsync(user);
        var tenantRole = tenant != null
            ? await _db.UserTenantRoles
                .Where(r => r.UserId == user.Id && r.TenantId == tenant.Id.ToString() && r.IsActive)
                .FirstOrDefaultAsync()
            : null;

        var effectiveRole = tenantRole?.Role.ToString() ?? identityRoles.FirstOrDefault() ?? string.Empty;

        var (rawNew, hashedNew) = GenerateRefreshToken();
        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = hashedNew;
        _db.RefreshTokens.Update(stored);

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TenantId = tenant?.Id.ToString() ?? stored.TenantId,
            TokenHash = hashedNew,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
            CreatedFromIp = ip
        };
        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync();

        var (jwt, jwtExpiry) = GenerateJwtToken(user, effectiveRole, tenant?.Id);

        await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.TokenRefreshed, ip, Request.Headers.UserAgent.ToString());

        SetAuthCookies(jwt, jwtExpiry, rawNew);

        return Ok(new { token = jwt, tokenExpiry = jwtExpiry, refreshToken = rawNew });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        var ip = GetClientIp();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var refreshToken = request?.RefreshToken ?? Request.Cookies["refresh_token"] ?? "";
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var tokenHash = HashToken(refreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.UserId == userId);
            if (stored != null && !stored.IsRevoked)
            {
                stored.IsRevoked = true;
                stored.RevokedAt = DateTime.UtcNow;
                _db.RefreshTokens.Update(stored);
                await _db.SaveChangesAsync();
            }
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        await WriteAuditAsync(userId, email, tenant?.Id, AuditEventType.Logout, ip, Request.Headers.UserAgent.ToString());

        ClearAuthCookies();

        return Ok(new { message = "Sesión cerrada correctamente." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound();

        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var identityRoles = await _userManager.GetRolesAsync(user);
        var tenantRole = tenant != null
            ? await _db.UserTenantRoles
                .Where(r => r.UserId == user.Id && r.TenantId == tenant.Id.ToString() && r.IsActive)
                .FirstOrDefaultAsync()
            : null;

        var effectiveRole = tenantRole?.Role.ToString() ?? identityRoles.FirstOrDefault() ?? string.Empty;

        return Ok(new
        {
            id = user.Id,
            name = user.FullName,
            email = user.Email,
            role = effectiveRole,
            isSuspended = user.Status == UserStatus.Suspended,
            lastLogin = user.LastLogin,
            tenantId = tenant?.Id,
            tenantName = tenant?.Name
        });
    }

    [HttpGet("my-tenants")]
    [Authorize]
    public async Task<IActionResult> GetMyTenants()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentTenant = HttpContext.Items["Tenant"] as Tenant;
        var now = DateTime.UtcNow;

        var roles = await _db.UserTenantRoles
            .Where(r => r.UserId == userId
                && r.IsActive)
            .ToListAsync();

        var options = new List<MyTenantOption>();

        foreach (var role in roles)
        {
            if (!Guid.TryParse(role.TenantId, out var parsedTenantId))
            {
                continue;
            }

            var tenant = await _masterDb.Tenants
                .FirstOrDefaultAsync(t => t.Id == parsedTenantId && t.IsActive);

            if (tenant == null)
            {
                continue;
            }

            var isCurrent = currentTenant != null && currentTenant.Id == tenant.Id;

            options.Add(new MyTenantOption(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                role.Role.ToString(),
                isCurrent));
        }

        return Ok(options.OrderBy(o => o.Name));
    }

    [HttpPost("switch-tenant")]
    [Authorize]
    public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequest request)
    {
        var ip = GetClientIp();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        var tenantRole = await _db.UserTenantRoles
            .Where(r => r.UserId == userId && r.TenantId == request.TenantId.ToString() && r.IsActive)
            .FirstOrDefaultAsync();

        if (tenantRole == null)
            return Forbid();

        var identityRoles = await _userManager.GetRolesAsync(user);
        var effectiveRole = tenantRole.Role.ToString();

        var (jwt, jwtExpiry) = GenerateJwtToken(user, effectiveRole, request.TenantId);
        var (rawRefresh, hashedRefresh) = GenerateRefreshToken();

        var newRefresh = new RefreshToken
        {
            UserId = user.Id,
            TenantId = request.TenantId.ToString(),
            TokenHash = hashedRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
            CreatedFromIp = ip
        };
        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();

        await WriteAuditAsync(user.Id, user.Email!, request.TenantId, AuditEventType.ContextSwitched, ip,
            Request.Headers.UserAgent.ToString(),
            JsonSerializer.Serialize(new { targetTenantId = request.TenantId }));

        SetAuthCookies(jwt, jwtExpiry, rawRefresh);

        return Ok(new
        {
            token = jwt,
            tokenExpiry = jwtExpiry,
            refreshToken = rawRefresh,
            tenantId = request.TenantId,
            role = effectiveRole
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "No se pudo cambiar la contraseña.", errors });
        }

        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        await WriteAuditAsync(user.Id, user.Email!, tenant?.Id, AuditEventType.PasswordChanged,
            GetClientIp(), Request.Headers.UserAgent.ToString());

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    private async Task HandleFailedAttemptAsync(User user, Guid? tenantId, string? ip, string? userAgent)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (user.DailyLockoutResetDate != today)
        {
            user.DailyLockoutCount = 0;
            user.DailyLockoutResetDate = today;
        }

        user.AccessFailedCount++;

        if (user.AccessFailedCount >= MaxLoginAttempts)
        {
            user.DailyLockoutCount++;
            user.AccessFailedCount = 0;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);

            _logger.LogWarning("Account locked: {Email} | Daily lockouts: {Count}", user.Email, user.DailyLockoutCount);

            await WriteAuditAsync(user.Id, user.Email!, tenantId, AuditEventType.AccountLocked, ip, userAgent,
                JsonSerializer.Serialize(new { daily_lockout_count = user.DailyLockoutCount }));

            if (user.DailyLockoutCount >= MaxDailyLockouts)
            {
                user.Status = UserStatus.Suspended;
                user.SuspendedAt = DateTime.UtcNow;
                user.SuspendedReason = "Suspensión automática por 3 bloqueos en el mismo día.";

                _logger.LogWarning("Account suspended: {Email}", user.Email);

                await WriteAuditAsync(user.Id, user.Email!, tenantId, AuditEventType.AccountSuspended, ip, userAgent,
                    JsonSerializer.Serialize(new { reason = "auto_3_lockouts" }));
            }
        }
        else
        {
            await WriteAuditAsync(user.Id, user.Email!, tenantId, AuditEventType.LoginFailed, ip, userAgent,
                JsonSerializer.Serialize(new { failed_attempt = user.AccessFailedCount, max = MaxLoginAttempts }));
        }

        await _userManager.UpdateAsync(user);
    }

    private (string jwt, DateTime expiry) GenerateJwtToken(User user, string role, Guid? tenantId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, role)
        };

        if (tenantId.HasValue)
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"] ?? "SoftcoinpERP",
            audience: _configuration["JWT:Audience"] ?? "SoftcoinpERP",
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static (string raw, string hashed) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (raw, HashToken(raw));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task WriteAuditAsync(
        string? userId, string email, Guid? tenantId,
        AuditEventType eventType, string? ip = null, string? userAgent = null, string? details = null)
    {
        try
        {
            _db.AccessAuditLogs.Add(new AccessAuditLog
            {
                UserId = userId,
                Email = email,
                TenantId = tenantId?.ToString(),
                EventType = eventType,
                IpAddress = ip,
                UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent,
                Details = details
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write audit log for {EventType}, userId={UserId}", eventType, userId);
        }
    }

    private async Task RevokeAllUserTokensAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    private void SetAuthCookies(string jwt, DateTime jwtExpiry, string refreshToken)
    {
        var isSecure = Request.IsHttps;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = jwtExpiry
        };
        Response.Cookies.Append("auth_token", jwt, cookieOptions);

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(RefreshTokenDays)
        };
        Response.Cookies.Append("refresh_token", refreshToken, refreshOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("auth_token", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/" });
    }

    private string GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string? RefreshToken);
public record SwitchTenantRequest(Guid TenantId);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record MyTenantOption(Guid TenantId, string Name, string Subdomain, string Role, bool IsCurrent);
