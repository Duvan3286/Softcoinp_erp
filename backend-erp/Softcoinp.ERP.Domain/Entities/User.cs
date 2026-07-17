using Microsoft.AspNetCore.Identity;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLogin { get; set; }

    public int DailyLockoutCount { get; set; } = 0;
    public DateOnly? DailyLockoutResetDate { get; set; }

    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedReason { get; set; }

    public string? TenantId { get; set; }

    public ICollection<UserTenantRole> TenantRoles { get; set; } = new List<UserTenantRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AccessAuditLog> AuditLogs { get; set; } = new List<AccessAuditLog>();
    public ICollection<UserEmailVerification> EmailVerifications { get; set; } = new List<UserEmailVerification>();
}
