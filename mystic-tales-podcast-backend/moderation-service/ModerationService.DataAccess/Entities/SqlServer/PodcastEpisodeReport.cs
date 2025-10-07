using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class PodcastEpisodeReport
{
    public Guid Id { get; set; }

    public string? Content { get; set; }

    public int AccountId { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public int PodcastEpisodeReportTypeId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisodeReportType PodcastEpisodeReportType { get; set; } = null!;
}
