using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeLicense
{
    public Guid Id { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public string LicenseDocumentFileKey { get; set; } = null!;

    public int PodcastEpisodeLicenseTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;

    public virtual PodcastEpisodeLicenseType PodcastEpisodeLicenseType { get; set; } = null!;
}
