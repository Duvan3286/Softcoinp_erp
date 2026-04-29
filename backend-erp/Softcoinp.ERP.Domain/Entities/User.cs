using Microsoft.AspNetCore.Identity;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Represents a user in the system, extending the base IdentityUser.
/// </summary>
public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
