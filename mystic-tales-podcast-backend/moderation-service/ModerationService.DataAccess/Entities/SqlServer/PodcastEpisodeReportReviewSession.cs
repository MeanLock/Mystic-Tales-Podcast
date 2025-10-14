using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeReportReviewSession
{
    public Guid Id { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public int AssignedStaff { get; set; }

    public bool? IsResolved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
