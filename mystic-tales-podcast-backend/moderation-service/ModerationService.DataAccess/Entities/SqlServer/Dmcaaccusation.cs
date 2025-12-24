using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class Dmcaaccusation
{
    public int Id { get; set; }

    public Guid? PodcastShowId { get; set; }

    public Guid? PodcastEpisodeId { get; set; }

    public int? AssignedStaff { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string AccuserEmail { get; set; } = null!;

    public string AccuserPhone { get; set; } = null!;

    public string AccuserFullName { get; set; } = null!;

    public string? DismissReason { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual ICollection<CounterNotice> CounterNotices { get; set; } = new List<CounterNotice>();

    public virtual ICollection<DmcaaccusationConclusionReport> DmcaaccusationConclusionReports { get; set; } = new List<DmcaaccusationConclusionReport>();

    public virtual ICollection<DmcaaccusationStatusTracking> DmcaaccusationStatusTrackings { get; set; } = new List<DmcaaccusationStatusTracking>();

    public virtual ICollection<Dmcanotice> Dmcanotices { get; set; } = new List<Dmcanotice>();

    public virtual ICollection<LawsuitProof> LawsuitProofs { get; set; } = new List<LawsuitProof>();
}
