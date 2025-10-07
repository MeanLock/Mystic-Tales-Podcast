using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastEpisodePublishReviewSessionStatusTracking
{
    public Guid Id { get; set; }

    public int PodcastEpisodePublishReviewSessionId { get; set; }

    public int PodcastEpisodePublishReviewSessionStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisodePublishReviewSession PodcastEpisodePublishReviewSession { get; set; } = null!;

    public virtual PodcastEpisodePublishReviewSessionStatus PodcastEpisodePublishReviewSessionStatus { get; set; } = null!;
}
