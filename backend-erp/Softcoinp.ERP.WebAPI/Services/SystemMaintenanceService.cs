using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class SystemMaintenanceService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<SystemMaintenanceService> _logger;

    public SystemMaintenanceService(
        UserManager<User> userManager,
        ApplicationDbContext context,
        ITenantResolver tenantResolver,
        ILogger<SystemMaintenanceService> logger)
    {
        _userManager = userManager;
        _context = context;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(
        string fullName, string email, string password, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email && u.TenantId == tenantId && u.Status != UserStatus.Deleted);
        if (emailExists)
        {
            throw new InvalidOperationException("El correo electrónico ya está registrado en este conjunto.");
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            Status = UserStatus.Active,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => TranslatePasswordError(e.Description)));
            _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
            throw new InvalidOperationException($"No se pudo crear el usuario: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Admin role to user {Email}, cleaning up", email);
            await _userManager.DeleteAsync(user);
            var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"No se pudo asignar el rol: {roleErrors}");
        }

        var tenantRole = new UserTenantRole
        {
            UserId = user.Id,
            TenantId = tenantId,
            Role = AppRole.Admin,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = performedByUserId
        };
        _context.UserTenantRoles.Add(tenantRole);

        var historyRecord = new UserChangeHistory
        {
            UserId = user.Id,
            ChangedField = "Account",
            OldValue = null,
            NewValue = $"User created with email {email}",
            ChangeType = UserChangeType.Created,
            ChangedByUserId = performedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.UserChangeHistories.Add(historyRecord);

        await _context.SaveChangesAsync();

        _logger.LogInformation("User created: {Email} in tenant {TenantId} by {AdminId}", email, tenantId, performedByUserId);

        return user;
    }

    public async Task<User> EditUserAsync(
        string targetUserId, string? newFullName, string? newEmail, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && u.TenantId == tenantId && u.Status != UserStatus.Deleted);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        if (!string.IsNullOrEmpty(newFullName) && newFullName != user.FullName)
        {
            var oldValue = user.FullName;
            user.FullName = newFullName;

            var historyRecord = new UserChangeHistory
            {
                UserId = user.Id,
                ChangedField = "FullName",
                OldValue = oldValue,
                NewValue = newFullName,
                ChangeType = UserChangeType.Edited,
                ChangedByUserId = performedByUserId,
                ChangedAt = DateTime.UtcNow
            };
            _context.UserChangeHistories.Add(historyRecord);
        }

        if (!string.IsNullOrEmpty(newEmail) && newEmail != user.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == newEmail && u.TenantId == tenantId && u.Id != targetUserId && u.Status != UserStatus.Deleted);
            if (emailExists)
            {
                throw new InvalidOperationException("El nuevo correo electrónico ya está registrado en este conjunto.");
            }

            var oldEmail = user.Email;
            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = _userManager.NormalizeEmail(newEmail);
            user.NormalizedUserName = _userManager.NormalizeName(newEmail);

            var historyRecord = new UserChangeHistory
            {
                UserId = user.Id,
                ChangedField = "Email",
                OldValue = oldEmail,
                NewValue = newEmail,
                ChangeType = UserChangeType.Edited,
                ChangedByUserId = performedByUserId,
                ChangedAt = DateTime.UtcNow
            };
            _context.UserChangeHistories.Add(historyRecord);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User edited: {UserId} in tenant {TenantId} by {AdminId}", targetUserId, tenantId, performedByUserId);

        return user;
    }

    public async Task DeleteUserAsync(string targetUserId, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        if (targetUserId == performedByUserId)
        {
            throw new InvalidOperationException("No puedes eliminarte a ti mismo.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && u.TenantId == tenantId);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(t => t.UserId == targetUserId && !t.IsRevoked)
                .ToListAsync();
            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            var tenantRoles = await _context.UserTenantRoles
                .Where(r => r.UserId == targetUserId)
                .ToListAsync();
            _context.UserTenantRoles.RemoveRange(tenantRoles);

            var emailVerifications = await _context.UserEmailVerifications
                .Where(v => v.UserId == targetUserId)
                .ToListAsync();
            _context.UserEmailVerifications.RemoveRange(emailVerifications);

            var identityResult = await _userManager.DeleteAsync(user);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo eliminar el usuario: {errors}");
            }

            var deletionHistory = new UserChangeHistory
            {
                UserId = targetUserId,
                ChangedField = "Account",
                OldValue = user.Email,
                NewValue = "User deleted",
                ChangeType = UserChangeType.Deleted,
                ChangedByUserId = performedByUserId,
                ChangedAt = DateTime.UtcNow
            };
            _context.UserChangeHistories.Add(deletionHistory);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("User deleted: {UserId} ({Email}) from tenant {TenantId} by {AdminId}",
                targetUserId, user.Email, tenantId, performedByUserId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SuspendUserAsync(string targetUserId, string? reason, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        if (targetUserId == performedByUserId)
        {
            throw new InvalidOperationException("No puedes suspenderte a ti mismo.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && u.TenantId == tenantId && u.Status != UserStatus.Deleted);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        if (user.Status == UserStatus.Suspended)
        {
            throw new InvalidOperationException("El usuario ya está suspendido.");
        }

        user.Status = UserStatus.Suspended;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendedReason = reason;
        user.UpdatedAt = DateTime.UtcNow;

        var refreshTokens = await _context.RefreshTokens
            .Where(t => t.UserId == targetUserId && !t.IsRevoked)
            .ToListAsync();
        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        var historyRecord = new UserChangeHistory
        {
            UserId = user.Id,
            ChangedField = "Status",
            OldValue = "Active",
            NewValue = "Suspended",
            ChangeType = UserChangeType.Suspended,
            ChangedByUserId = performedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.UserChangeHistories.Add(historyRecord);

        await _context.SaveChangesAsync();

        _logger.LogInformation("User suspended: {UserId} in tenant {TenantId} by {AdminId}. Reason: {Reason}",
            targetUserId, tenantId, performedByUserId, reason ?? "None");
    }

    public async Task ReactivateUserAsync(string targetUserId, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && u.TenantId == tenantId && u.Status != UserStatus.Deleted);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        if (user.Status != UserStatus.Suspended)
        {
            throw new InvalidOperationException("El usuario no está suspendido.");
        }

        user.Status = UserStatus.Active;
        user.SuspendedAt = null;
        user.SuspendedReason = null;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        var historyRecord = new UserChangeHistory
        {
            UserId = user.Id,
            ChangedField = "Status",
            OldValue = "Suspended",
            NewValue = "Active",
            ChangeType = UserChangeType.Reactivated,
            ChangedByUserId = performedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.UserChangeHistories.Add(historyRecord);

        await _context.SaveChangesAsync();

        _logger.LogInformation("User reactivated: {UserId} in tenant {TenantId} by {AdminId}",
            targetUserId, tenantId, performedByUserId);
    }

    public async Task ResetPasswordAsync(string targetUserId, string newPassword, string performedByUserId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context available.");
        }

        var tenantId = tenant.Id.ToString();

        if (targetUserId == performedByUserId)
        {
            throw new InvalidOperationException("No puedes restablecer tu propia contraseña desde este módulo.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId && u.TenantId == tenantId && u.Status == UserStatus.Active);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado o no está activo.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join("; ", resetResult.Errors.Select(e => TranslatePasswordError(e.Description)));
            _logger.LogError("Failed to reset password for user {UserId}: {Errors}", targetUserId, errors);
            throw new InvalidOperationException($"No se pudo restablecer la contraseña: {errors}");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        var refreshTokens = await _context.RefreshTokens
            .Where(t => t.UserId == targetUserId && !t.IsRevoked)
            .ToListAsync();
        foreach (var tokenEntry in refreshTokens)
        {
            tokenEntry.IsRevoked = true;
            tokenEntry.RevokedAt = DateTime.UtcNow;
        }

        var historyRecord = new UserChangeHistory
        {
            UserId = user.Id,
            ChangedField = "Password",
            OldValue = null,
            NewValue = "Password reset by superuser",
            ChangeType = UserChangeType.PasswordReset,
            ChangedByUserId = performedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        _context.UserChangeHistories.Add(historyRecord);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Password reset for user: {UserId} in tenant {TenantId} by {AdminId}",
            targetUserId, tenantId, performedByUserId);
    }

    public async Task<List<User>> GetUsersAsync(string tenantId, string? search = null, string? sortBy = null, string? sortOrder = null)
    {
        var query = _context.Users
            .Where(u => u.TenantId == tenantId && u.Status != UserStatus.Deleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(searchLower) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)));
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "fullname" => sortOrder == "desc" ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "email" => sortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "status" => sortOrder == "desc" ? query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status),
            "createdat" => sortOrder == "desc" ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "lastlogin" => sortOrder == "desc" ? query.OrderByDescending(u => u.LastLogin) : query.OrderBy(u => u.LastLogin),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<List<UserChangeHistory>> GetUserChangeHistoryAsync(string userId, string tenantId)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId && u.TenantId == tenantId);
        if (!userExists)
        {
            var historyExists = await _context.UserChangeHistories
                .AnyAsync(h => h.UserId == userId);
            if (!historyExists)
            {
                return new List<UserChangeHistory>();
            }
        }

        return await _context.UserChangeHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<bool> VerifyEmailAsync(string rawToken)
    {
        var tokenHash = ComputeSha256Hash(rawToken);

        var verification = await _context.UserEmailVerifications
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.TokenHash == tokenHash && !v.IsVerified);

        if (verification == null)
        {
            return false;
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        var user = verification.User;
        if (user == null)
        {
            return false;
        }

        var oldEmail = user.Email;
        user.Email = verification.NewEmail;
        user.UserName = verification.NewEmail;
        user.NormalizedEmail = _userManager.NormalizeEmail(verification.NewEmail);
        user.NormalizedUserName = _userManager.NormalizeName(verification.NewEmail);
        user.UpdatedAt = DateTime.UtcNow;

        verification.IsVerified = true;
        verification.VerifiedAt = DateTime.UtcNow;

        var pendingHistory = await _context.UserChangeHistories
            .FirstOrDefaultAsync(h =>
                h.UserId == user.Id &&
                h.ChangedField == "Email" &&
                h.NewValue != null &&
                h.NewValue.Contains("pending verification") &&
                h.NewValue.Contains(verification.NewEmail));

        if (pendingHistory != null)
        {
            pendingHistory.NewValue = $"{verification.NewEmail} (verified on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC)";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email verified for user {UserId}: {OldEmail} -> {NewEmail}",
            user.Id, oldEmail, verification.NewEmail);

        return true;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string TranslatePasswordError(string error)
    {
        return error switch
        {
            "Passwords must have at least one non alphanumeric character." =>
                "La contraseña debe contener al menos un carácter no alfanumérico.",
            "Passwords must have at least one lowercase ('a'-'z')." =>
                "La contraseña debe contener al menos una letra minúscula ('a'-'z').",
            "Passwords must have at least one uppercase ('A'-'Z')." =>
                "La contraseña debe contener al menos una letra mayúscula ('A'-'Z').",
            "Passwords must have at least one digit ('0'-'9')." =>
                "La contraseña debe contener al menos un dígito ('0'-'9').",
            "Passwords must be at least 6 characters." =>
                "La contraseña debe tener al menos 6 caracteres.",
            "Passwords must be at least 8 characters." =>
                "La contraseña debe tener al menos 8 caracteres.",
            _ => error
        };
    }
}
