using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastEpisodeLicenseType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastEpisodeLicense> PodcastEpisodeLicenses { get; set; } = new List<PodcastEpisodeLicense>();
}
