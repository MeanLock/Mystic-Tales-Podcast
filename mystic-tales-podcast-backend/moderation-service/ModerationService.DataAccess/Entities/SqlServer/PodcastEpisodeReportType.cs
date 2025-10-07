using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class PodcastEpisodeReportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastEpisodeReport> PodcastEpisodeReports { get; set; } = new List<PodcastEpisodeReport>();
}
