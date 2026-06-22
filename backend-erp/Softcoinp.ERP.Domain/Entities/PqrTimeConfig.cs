using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrTimeConfig
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public PQRType PQRType { get; set; }
    public int BusinessDays { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedByUserId { get; set; } = string.Empty;
}
