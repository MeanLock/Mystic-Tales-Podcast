using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class CounterNotice
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public string AccountEmail { get; set; } = null!;

    public string AccountPhone { get; set; } = null!;

    public string StatementPerjury { get; set; } = null!;

    public string Signature { get; set; } = null!;

    public int DmcaAccusationId { get; set; }

    public string Jurisdiction { get; set; } = null!;

    public bool? IsValid { get; set; }

    public string? InvalidReason { get; set; }

    public int? ValidatedBy { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public DateOnly FiledDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CounterNoticeAttachFile> CounterNoticeAttachFiles { get; set; } = new List<CounterNoticeAttachFile>();

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;
}
