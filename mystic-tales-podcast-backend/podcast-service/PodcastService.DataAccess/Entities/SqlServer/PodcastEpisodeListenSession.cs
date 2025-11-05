using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeListenSession
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public int LastListenDurationSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsContentRemoved { get; set; }

    public int? PodcastCategoryId { get; set; }

    public int? PodcastSubCategoryId { get; set; }

    public virtual PodcastCategory? PodcastCategory { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;

    public virtual ICollection<PodcastEpisodeListenSessionHlsEnckeyRequestToken> PodcastEpisodeListenSessionHlsEnckeyRequestTokens { get; set; } = new List<PodcastEpisodeListenSessionHlsEnckeyRequestToken>();

    public virtual PodcastSubCategory? PodcastSubCategory { get; set; }
}
