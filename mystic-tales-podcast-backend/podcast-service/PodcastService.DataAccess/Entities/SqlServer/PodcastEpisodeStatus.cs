using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastEpisodeStatusTracking> PodcastEpisodeStatusTrackings { get; set; } = new List<PodcastEpisodeStatusTracking>();
}
