using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrInternalNote
{
    public Guid Id { get; set; }

    public Guid PQRId { get; set; }
    public PqrRecord? PQR { get; set; }

    public string NoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<PqrFile> Files { get; set; } = new List<PqrFile>();
}
