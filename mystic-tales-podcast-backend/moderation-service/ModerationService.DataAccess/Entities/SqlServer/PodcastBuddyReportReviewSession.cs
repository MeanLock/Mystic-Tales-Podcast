using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class PodcastBuddyReportReviewSession
{
    public Guid Id { get; set; }

    public int PodcastBuddyId { get; set; }

    public int AssignedStaff { get; set; }

    public int ResolvedViolationPoint { get; set; }

    public bool? IsResolved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
