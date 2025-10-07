using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class PodcastShowReport
{
    public Guid Id { get; set; }

    public string? Content { get; set; }

    public int AccountId { get; set; }

    public Guid PodcastShowId { get; set; }

    public int PodcastShowReportTypeId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastShowReportType PodcastShowReportType { get; set; } = null!;
}
