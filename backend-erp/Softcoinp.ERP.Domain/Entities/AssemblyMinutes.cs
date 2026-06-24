using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyMinutes
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public MinutesStatus Status { get; set; } = MinutesStatus.Draft;

    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }

    public string FullText { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }
    public string GeneratedByUserId { get; set; } = string.Empty;

    public string? CommissionMemberNames { get; set; }
    public DateTime? CommissionReviewDeadline { get; set; }
    public string? CommissionComments { get; set; }

    public string? PresidentSignatureFilePath { get; set; }
    public string? SecretarySignatureFilePath { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }

    public DateTime? PublishedAt { get; set; }
    public string? PublishedByUserId { get; set; }
    public int? PublishNotificationCount { get; set; }

    public string? RevisionNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}
