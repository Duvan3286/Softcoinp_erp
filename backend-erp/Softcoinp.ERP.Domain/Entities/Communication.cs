using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Communication : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public CommunicationStatus Status { get; set; } = CommunicationStatus.Draft;
    public AudienceType AudienceType { get; set; }
    public string SelectedChannels { get; set; } = string.Empty;

    public DateTime? SendAt { get; set; }
    public DateTime? SentAt { get; set; }

    public bool RequiresReadConfirmation { get; set; }
    public bool PublishToBulletinBoard { get; set; }

    public Guid? RelatedCommunicationId { get; set; }
    public Communication? RelatedCommunication { get; set; }

    public string FilePaths { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserId { get; set; }

    public ICollection<CommunicationRecipient> Recipients { get; set; } = new List<CommunicationRecipient>();
}
