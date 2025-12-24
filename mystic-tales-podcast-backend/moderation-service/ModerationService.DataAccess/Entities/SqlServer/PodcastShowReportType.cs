using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class PodcastShowReportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastShowReport> PodcastShowReports { get; set; } = new List<PodcastShowReport>();
}
