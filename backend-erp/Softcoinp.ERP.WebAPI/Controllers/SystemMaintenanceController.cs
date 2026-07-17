using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("api/v1/maintenance")]
public class SystemMaintenanceController : ControllerBase
{
    private readonly SystemMaintenanceService _maintenanceService;
    private readonly ILogger<SystemMaintenanceController> _logger;

    public SystemMaintenanceController(
        SystemMaintenanceService maintenanceService,
        ILogger<SystemMaintenanceController> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder)
    {
        var tenantId = User.FindFirstValue("tenant_id") ?? string.Empty;

        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest(new { message = "Tenant context not available." });
        }

        var users = await _maintenanceService.GetUsersAsync(tenantId, search, sortBy, sortOrder);

        var result = users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            Status = u.Status.ToString(),
            IsSuspended = u.Status == UserStatus.Suspended,
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin
        }).ToList();

        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var tenantId = User.FindFirstValue("tenant_id") ?? string.Empty;
        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest(new { message = "Tenant context not available." });
        }

        var users = await _maintenanceService.GetUsersAsync(tenantId);
        var user = users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(new UserDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLogin = user.LastLogin,
            IsSuspended = user.Status == UserStatus.Suspended,
            SuspendedAt = user.SuspendedAt,
            SuspendedReason = user.SuspendedReason
        });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "El nombre completo es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "El correo electrónico es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "La contraseña es obligatoria." });
        }

        if (request.Password.Length < 8)
        {
            return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres." });
        }

        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            var createdUser = await _maintenanceService.CreateUserAsync(
                request.FullName.Trim(),
                request.Email.Trim().ToLowerInvariant(),
                request.Password,
                performedByUserId);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new CreateUserResponse
            {
                UserId = createdUser.Id,
                FullName = createdUser.FullName,
                Email = createdUser.Email ?? string.Empty,
                Message = "Usuario creado exitosamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> EditUser(string id, [FromBody] EditUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) && string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Debes proporcionar al menos un campo para actualizar." });
        }

        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            var user = await _maintenanceService.EditUserAsync(
                id,
                request.FullName?.Trim(),
                request.Email?.Trim().ToLowerInvariant(),
                performedByUserId);

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLogin = user.LastLogin
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await _maintenanceService.DeleteUserAsync(id, performedByUserId);
            return Ok(new { message = "Usuario eliminado permanentemente." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/{id}/suspend")]
    public async Task<IActionResult> SuspendUser(string id, [FromBody] SuspendUserRequest request)
    {
        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await _maintenanceService.SuspendUserAsync(id, request.Reason, performedByUserId);
            return Ok(new { message = "Usuario suspendido correctamente." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/{id}/reactivate")]
    public async Task<IActionResult> ReactivateUser(string id)
    {
        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await _maintenanceService.ReactivateUserAsync(id, performedByUserId);
            return Ok(new { message = "Usuario reactivado correctamente." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "La nueva contraseña es obligatoria." });
        }

        if (request.NewPassword.Length < 8)
        {
            return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres." });
        }

        var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            await _maintenanceService.ResetPasswordAsync(id, request.NewPassword, performedByUserId);
            return Ok(new { message = "Contraseña restablecida correctamente." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("users/{id}/history")]
    public async Task<IActionResult> GetUserHistory(string id)
    {
        var tenantId = User.FindFirstValue("tenant_id") ?? string.Empty;
        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest(new { message = "Tenant context not available." });
        }

        var history = await _maintenanceService.GetUserChangeHistoryAsync(id, tenantId);

        var result = history.Select(h => new UserChangeHistoryDto
        {
            Id = h.Id,
            ChangedField = h.ChangedField,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            ChangedAt = h.ChangedAt,
            ChangeType = h.ChangeType.ToString(),
            ChangedByUserId = h.ChangedByUserId
        }).ToList();

        return Ok(result);
    }
}
