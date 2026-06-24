using System;

namespace Softcoinp.ERP.Domain.Entities;

public class PDFTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public string ReportTypeCode { get; set; } = string.Empty;
    public string? LogoFilePath { get; set; }
    public string HeaderText { get; set; } = string.Empty;
    public string FooterText { get; set; } = string.Empty;
    public string SignatureName { get; set; } = string.Empty;
    public string SignatureRole { get; set; } = string.Empty;
    public string? ConfidentialityNote { get; set; }
    public string? DisclaimerNote { get; set; }

    public string PrimaryColor { get; set; } = "#059669";
    public string SecondaryColor { get; set; } = "#1e293b";

    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
