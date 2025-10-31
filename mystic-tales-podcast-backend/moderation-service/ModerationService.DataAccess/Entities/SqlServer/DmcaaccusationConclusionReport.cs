using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class DmcaaccusationConclusionReport
{
    public Guid Id { get; set; }

    public int DmcaAccusationId { get; set; }

    public int DmcaAccusationConclusionReportTypeId { get; set; }

    public string? Description { get; set; }

    public string? InvalidReason { get; set; }

    public bool? IsRejected { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;

    public virtual DmcaaccusationConclusionReportType DmcaAccusationConclusionReportType { get; set; } = null!;
}
