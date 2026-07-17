namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Tracks email change verification requests.
/// When a superuser changes a user's email, a verification token is sent.
/// The change takes effect only after the user verifies the new email.
/// </summary>
public class UserEmailVerification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID of the user whose email is being changed.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>The new email address pending verification.</summary>
    public string NewEmail { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the verification token sent to the new email.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Expiration date of the verification token.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Whether the email has been verified.</summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>Timestamp when the verification was completed.</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}
