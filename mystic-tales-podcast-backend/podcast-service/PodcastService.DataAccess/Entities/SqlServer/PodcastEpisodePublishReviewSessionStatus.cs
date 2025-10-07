using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastEpisodePublishReviewSessionStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastEpisodePublishReviewSessionStatusTracking> PodcastEpisodePublishReviewSessionStatusTrackings { get; set; } = new List<PodcastEpisodePublishReviewSessionStatusTracking>();
}
