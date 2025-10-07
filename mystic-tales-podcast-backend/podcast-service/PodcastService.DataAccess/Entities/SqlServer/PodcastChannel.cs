using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastChannel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? BackgroundImageFileKey { get; set; }

    public string? MainImageFileKey { get; set; }

    public int TotalFavorite { get; set; }

    public int ListenCount { get; set; }

    public int PodcasterId { get; set; }

    public int? PodcastCategoryId { get; set; }

    public int? PodcastSubCategoryId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastCategory? PodcastCategory { get; set; }

    public virtual ICollection<PodcastChannelStatusTracking> PodcastChannelStatusTrackings { get; set; } = new List<PodcastChannelStatusTracking>();

    public virtual ICollection<PodcastShow> PodcastShows { get; set; } = new List<PodcastShow>();

    public virtual PodcastSubCategory? PodcastSubCategory { get; set; }

    public virtual ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
}
