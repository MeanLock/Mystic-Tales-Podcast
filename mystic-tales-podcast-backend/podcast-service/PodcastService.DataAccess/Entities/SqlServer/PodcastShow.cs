using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastShow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Language { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public bool? IsReleased { get; set; }

    public string Copyright { get; set; } = null!;

    public string? UploadFrequency { get; set; }

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }

    public string? MainImageFileKey { get; set; }

    public string? TrailerAudioFileKey { get; set; }

    public int TotalFollow { get; set; }

    public int ListenCount { get; set; }

    public int PodcasterId { get; set; }

    public int? PodcastCategoryId { get; set; }

    public int? PodcastSubCategoryId { get; set; }

    public int PodcastShowsSubscriptionTypeId { get; set; }

    public Guid? PodcastChannelId { get; set; }

    public string? TakenDownReason { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastCategory? PodcastCategory { get; set; }

    public virtual PodcastChannel? PodcastChannel { get; set; }

    public virtual ICollection<PodcastEpisode> PodcastEpisodes { get; set; } = new List<PodcastEpisode>();

    public virtual ICollection<PodcastShowReview> PodcastShowReviews { get; set; } = new List<PodcastShowReview>();

    public virtual ICollection<PodcastShowStatusTracking> PodcastShowStatusTrackings { get; set; } = new List<PodcastShowStatusTracking>();

    public virtual PodcastShowsSubscriptionType PodcastShowsSubscriptionType { get; set; } = null!;

    public virtual PodcastSubCategory? PodcastSubCategory { get; set; }

    public virtual ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
}
