using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/invitations")]
public class InvitationController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<InvitationController> _logger;

    public InvitationController(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ITenantResolver tenantResolver,
        ILogger<InvitationController> logger)
    {
        _db = db;
        _userManager = userManager;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/invitations
    // ─────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize] // Require Auth
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null) return BadRequest("No tenant active.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Verificar permisos del usuario actual (SuperAdmin o Admin)
        var currentUser = await _userManager.FindByIdAsync(userId!);
        if (currentUser == null) return BadRequest("Usuario no encontrado.");
        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var tenantRole = await _db.UserTenantRoles
            .FirstOrDefaultAsync(r => r.UserId == userId && r.TenantId == tenant.Id.ToString() && r.IsActive);

        var effectiveRoleStr = tenantRole?.Role.ToString() ?? userRoles.FirstOrDefault();
        if (effectiveRoleStr != nameof(AppRole.SuperAdmin) && effectiveRoleStr != nameof(AppRole.Admin))
            return Forbid();

        if (!Enum.TryParse<AppRole>(request.Role, true, out var roleEnum))
            return BadRequest("Rol inválido.");

        // Check si el usuario ya existe en este tenant
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            var existingRole = await _db.UserTenantRoles
                .AnyAsync(r => r.UserId == existingUser.Id && r.TenantId == tenant.Id.ToString() && r.IsActive);
            if (existingRole)
                return BadRequest("El usuario ya pertenece a este conjunto.");
        }

        // Generar Token
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
        var hashedToken = HashToken(rawToken);

        var invitation = new Invitation
        {
            Email = request.Email,
            TenantId = tenant.Id.ToString(),
            Role = roleEnum,
            TokenHash = hashedToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId!
        };

        _db.Invitations.Add(invitation);
        
        // Registro en auditoría
        _db.AccessAuditLogs.Add(new AccessAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Email = request.Email,
            TenantId = tenant.Id.ToString(),
            EventType = AuditEventType.InvitationSent,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Details = $"Invitación creada para el rol {request.Role}"
        });

        await _db.SaveChangesAsync();

        // ── AQUÍ SE ENVIARÍA EL EMAIL ──
        // Para desarrollo, mostramos el enlace en los logs
        var inviteLink = $"{Request.Scheme}://{Request.Host}/invite/{rawToken}";
        _logger.LogInformation("==========================================");
        _logger.LogInformation("INVITATION LINK GENERATED: {Link}", inviteLink);
        _logger.LogInformation("==========================================");

        return Ok(new { message = "Invitación generada correctamente. Enlace logueado en consola." });
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/invitations/{token}
    // ─────────────────────────────────────────────────────────────────
    [HttpGet("{token}")]
    public async Task<IActionResult> ValidateInvitation(string token)
    {
        var hashedToken = HashToken(token);
        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.TokenHash == hashedToken);

        if (invitation == null)
            return NotFound(new { message = "Invitación no encontrada." });

        if (invitation.Status != InvitationStatus.Pending)
            return BadRequest(new { message = $"La invitación ya no es válida (Estado: {invitation.Status})." });

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _db.SaveChangesAsync();
            return BadRequest(new { message = "La invitación ha expirado." });
        }

        return Ok(new
        {
            email = invitation.Email,
            role = invitation.Role.ToString(),
            tenantId = invitation.TenantId
        });
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/invitations/{token}/accept
    // ─────────────────────────────────────────────────────────────────
    [HttpPost("{token}/accept")]
    public async Task<IActionResult> AcceptInvitation(string token, [FromBody] AcceptInvitationRequest request)
    {
        var hashedToken = HashToken(token);
        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.TokenHash == hashedToken);

        if (invitation == null || invitation.Status != InvitationStatus.Pending || invitation.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "Invitación inválida o expirada." });

        var user = await _userManager.FindByEmailAsync(invitation.Email);

        if (user == null)
        {
            // Crear usuario nuevo
            user = new User
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                FullName = request.FullName,
                EmailConfirmed = true,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = "Error creando usuario.", errors = result.Errors.Select(e => e.Description) });
        }
        else
        {
            // Usuario ya existe: actualizar nombre si viene. NO resetear contraseña
            // (el admin no debería poder cambiar la contraseña de otro usuario por invitación)
            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;
            await _userManager.UpdateAsync(user);
        }

        // Asignar el rol en el tenant específico
        var userRole = new UserTenantRole
        {
            UserId = user.Id,
            TenantId = invitation.TenantId,
            Role = invitation.Role,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = invitation.CreatedByUserId
        };

        _db.UserTenantRoles.Add(userRole);

        // Actualizar invitación
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = user.Id;

        _db.Invitations.Update(invitation);

        // Auditoría
        _db.AccessAuditLogs.Add(new AccessAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            TenantId = invitation.TenantId,
            EventType = AuditEventType.InvitationAccepted,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Details = $"Invitación aceptada. Rol asignado: {invitation.Role}"
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Cuenta activada correctamente. Ya puedes iniciar sesión." });
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public record CreateInvitationRequest(string Email, string Role);
public record AcceptInvitationRequest(string FullName, string Password);
