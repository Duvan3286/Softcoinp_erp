using System;

namespace Softcoinp.ERP.Domain.Entities;

public enum TenantDocumentType
{
    HorizontalPropertyRegulation,
    LegalRepresentationCertificate,
    Rut,
    Other
}

public class TenantDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public TenantDocumentType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
}
