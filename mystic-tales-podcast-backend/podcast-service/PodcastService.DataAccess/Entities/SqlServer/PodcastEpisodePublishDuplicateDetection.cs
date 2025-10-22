using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodePublishDuplicateDetection
{
    public int PodcastEpisodePublishReviewSessionId { get; set; }

    public Guid DuplicatePodcastEpisodeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisode DuplicatePodcastEpisode { get; set; } = null!;

    public virtual PodcastEpisodePublishReviewSession PodcastEpisodePublishReviewSession { get; set; } = null!;
}
