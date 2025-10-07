using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastShowReview
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public double Rating { get; set; }

    public int AccountId { get; set; }

    public Guid PodcastShowId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastShow PodcastShow { get; set; } = null!;
}
