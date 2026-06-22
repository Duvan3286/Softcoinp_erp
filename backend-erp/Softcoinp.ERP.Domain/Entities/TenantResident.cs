using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class TenantResident
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }

    public string? RealEstateAgentName { get; set; }
    public string? RealEstateAgentPhone { get; set; }

    public bool AuthorizedToPayAdmin { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;

    // Navigation properties for PQR module
    public System.Collections.Generic.ICollection<PqrRecord> PqrRecords { get; set; } = new System.Collections.Generic.List<PqrRecord>();
}
