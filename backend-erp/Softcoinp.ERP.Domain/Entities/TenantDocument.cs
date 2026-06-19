using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public enum DocumentType
{
    HorizontalPropertyRegulation, // Reglamento de Propiedad Horizontal
    LegalRepresentationCertificate, // Certificado de Existencia y Representación
    Rut, // RUT (Registro Único Tributario)
    Other
}

/// <summary>
/// Documentos oficiales del Conjunto (Reglamento, RUT, Certificado).
/// Con control de acceso basado en rol.
/// </summary>
public class TenantDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    
    public DocumentType Type { get; set; }

    /// <summary>Ruta del archivo en el storage local (/uploads/...)</summary>
    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";
    
    public long FileSize { get; set; }

    /// <summary>Rol mínimo requerido para poder descargar este documento</summary>
    public AppRole MinimumRoleRequired { get; set; } = AppRole.Admin;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string UploadedByUserId { get; set; } = string.Empty;
}
