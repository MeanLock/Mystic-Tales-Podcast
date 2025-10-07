using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastChannelStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastChannelStatusTracking> PodcastChannelStatusTrackings { get; set; } = new List<PodcastChannelStatusTracking>();
}
