using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyAgendaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public int SequenceNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PresenterName { get; set; }

    public MajorityType MajorityRequired { get; set; } = MajorityType.Simple;
    public VotingMode VotingMode { get; set; } = VotingMode.Public;

    public bool IsInformationOnly { get; set; }
    public bool RequiresVoting { get; set; } = true;

    public decimal TotalCoefficientsForVote { get; set; }
    public decimal VotesInFavorCoefficients { get; set; }
    public decimal VotesAgainstCoefficients { get; set; }
    public decimal AbstentionCoefficients { get; set; }

    public int VotesInFavorCount { get; set; }
    public int VotesAgainstCount { get; set; }
    public int AbstentionCount { get; set; }

    public bool? IsApproved { get; set; }
    public string? RejectionReason { get; set; }

    public string? Observations { get; set; }
    public string? OwnerNotes { get; set; }

    public bool VoteRegistered { get; set; }
    public string? RegisteredByUserId { get; set; }
    public DateTime? VoteRegisteredAt { get; set; }
}
