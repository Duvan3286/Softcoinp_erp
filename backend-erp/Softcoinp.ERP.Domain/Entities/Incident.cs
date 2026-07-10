using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentType IncidentType { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public decimal TotalDamageValue { get; set; }
    public Guid? InsuranceContractId { get; set; }
    public Contract? InsuranceContract { get; set; }
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public string PolicyFilePath { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<IncidentWorkOrder> IncidentWorkOrders { get; set; } = new List<IncidentWorkOrder>();
}
