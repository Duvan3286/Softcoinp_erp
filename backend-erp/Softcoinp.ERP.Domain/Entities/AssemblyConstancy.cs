using System;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyConstancy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public Guid? AgendaItemId { get; set; }
    public AssemblyAgendaItem? AgendaItem { get; set; }

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public string OwnerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string RegisteredByUserId { get; set; } = string.Empty;
}
