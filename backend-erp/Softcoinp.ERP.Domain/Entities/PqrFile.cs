using System;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrFile
{
    public Guid Id { get; set; }

    public Guid PQRId { get; set; }
    public PqrRecord? PQR { get; set; }

    public Guid? PqrResponseId { get; set; }
    public PqrResponse? PqrResponse { get; set; }

    public Guid? PqrInternalNoteId { get; set; }
    public PqrInternalNote? PqrInternalNote { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public string UploadedByUserId { get; set; } = string.Empty;
    public string UploadedByUserName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsFromApplicant { get; set; }
}
