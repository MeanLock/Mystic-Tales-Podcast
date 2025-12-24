using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class LawsuitProof
{
    public Guid Id { get; set; }

    public int DmcaAccusationId { get; set; }

    public bool? IsValid { get; set; }

    public string? InValidReason { get; set; }

    public int? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;

    public virtual ICollection<LawsuitProofAttachFile> LawsuitProofAttachFiles { get; set; } = new List<LawsuitProofAttachFile>();
}
