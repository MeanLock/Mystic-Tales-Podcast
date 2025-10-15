using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisode
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool ExplicitContent { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public bool? IsReleased { get; set; }

    public string? MainImageFileKey { get; set; }

    public string AudioFileKey { get; set; } = null!;

    public double AudioFileSize { get; set; }

    public int AudioLength { get; set; }

    public byte[]? AudioFingerPrint { get; set; }

    public int PodcastEpisodeSubscriptionTypeId { get; set; }

    public Guid PodcastShowId { get; set; }

    public int SeasonNumber { get; set; }

    public int TotalSave { get; set; }

    public int ListenCount { get; set; }

    public bool? IsAudioPublishable { get; set; }

    public string? TakenDownReason { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int EpisodeOrder { get; set; }

    public virtual ICollection<PodcastEpisodeHashtag> PodcastEpisodeHashtags { get; set; } = new List<PodcastEpisodeHashtag>();

    public virtual ICollection<PodcastEpisodeIllegalContentTypeMarking> PodcastEpisodeIllegalContentTypeMarkings { get; set; } = new List<PodcastEpisodeIllegalContentTypeMarking>();

    public virtual ICollection<PodcastEpisodeLicense> PodcastEpisodeLicenses { get; set; } = new List<PodcastEpisodeLicense>();

    public virtual ICollection<PodcastEpisodePublishReviewSession> PodcastEpisodePublishReviewSessions { get; set; } = new List<PodcastEpisodePublishReviewSession>();

    public virtual ICollection<PodcastEpisodeStatusTracking> PodcastEpisodeStatusTrackings { get; set; } = new List<PodcastEpisodeStatusTracking>();

    public virtual PodcastEpisodeSubscriptionType PodcastEpisodeSubscriptionType { get; set; } = null!;

    public virtual PodcastShow PodcastShow { get; set; } = null!;
}
