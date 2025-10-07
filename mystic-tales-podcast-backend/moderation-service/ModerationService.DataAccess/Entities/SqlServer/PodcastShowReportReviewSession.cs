using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class PodcastShowReportReviewSession
{
    public Guid Id { get; set; }

    public Guid PodcastShowId { get; set; }

    public int AssignedStaff { get; set; }

    public bool? IsResolved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
