using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class PodcastBuddyReportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastBuddyReport> PodcastBuddyReports { get; set; } = new List<PodcastBuddyReport>();
}
