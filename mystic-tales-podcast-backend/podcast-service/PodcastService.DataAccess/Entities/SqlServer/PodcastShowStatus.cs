using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastShowStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastShowStatusTracking> PodcastShowStatusTrackings { get; set; } = new List<PodcastShowStatusTracking>();
}
