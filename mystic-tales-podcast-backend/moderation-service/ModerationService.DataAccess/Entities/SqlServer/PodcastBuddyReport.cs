using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class PodcastBuddyReport
{
    public Guid Id { get; set; }

    public string? Content { get; set; }

    public int AccountId { get; set; }

    public int PodcastBuddyId { get; set; }

    public int PodcastBuddyReportTypeId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastBuddyReportType PodcastBuddyReportType { get; set; } = null!;
}
