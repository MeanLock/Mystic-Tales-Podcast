using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class LawsuitProof
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public string GoodFaithStatement { get; set; } = null!;

    public string CourtName { get; set; } = null!;

    public string CaseNumber { get; set; } = null!;

    public DateOnly FilingDate { get; set; }

    public string Signature { get; set; } = null!;

    public int DmcaAccusationId { get; set; }

    public bool? IsValid { get; set; }

    public string? InValidReason { get; set; }

    public int? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public string? JudgmentDetails { get; set; }

    public DateOnly? DateResolved { get; set; }

    public string? Outcome { get; set; }

    public string? RulingDocumentFileUrl { get; set; }

    public bool? IsDefendantWon { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;

    public virtual ICollection<LawsuitProofAttachFile> LawsuitProofAttachFiles { get; set; } = new List<LawsuitProofAttachFile>();
}
