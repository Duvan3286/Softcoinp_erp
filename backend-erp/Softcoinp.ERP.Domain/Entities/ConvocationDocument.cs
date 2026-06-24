using System;

namespace Softcoinp.ERP.Domain.Entities;

public class ConvocationDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ConvocationId { get; set; }
    public AssemblyConvocation? Convocation { get; set; }

    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
