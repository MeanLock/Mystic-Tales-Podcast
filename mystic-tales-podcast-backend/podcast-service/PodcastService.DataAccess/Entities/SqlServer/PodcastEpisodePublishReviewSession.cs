using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodePublishReviewSession
{
    public int Id { get; set; }

    public int AssignedStaff { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public string? Note { get; set; }

    public int ReReviewCount { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;

    public virtual ICollection<PodcastEpisodePublishDuplicateDetection> PodcastEpisodePublishDuplicateDetections { get; set; } = new List<PodcastEpisodePublishDuplicateDetection>();

    public virtual ICollection<PodcastEpisodePublishReviewSessionStatusTracking> PodcastEpisodePublishReviewSessionStatusTrackings { get; set; } = new List<PodcastEpisodePublishReviewSessionStatusTracking>();
}
