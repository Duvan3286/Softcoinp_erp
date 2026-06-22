using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrResponse
{
    public Guid Id { get; set; }

    public Guid PQRId { get; set; }
    public PqrRecord? PQR { get; set; }

    public string ResponseText { get; set; } = string.Empty;
    public bool IsDefinitive { get; set; }
    public bool IsPartialUpdate { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string SentByUserId { get; set; } = string.Empty;
    public string SentByUserName { get; set; } = string.Empty;

    public bool RequiresConfirmation { get; set; }
    public bool? ConfirmedByRadiador { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public ICollection<PqrFile> Files { get; set; } = new List<PqrFile>();
}
