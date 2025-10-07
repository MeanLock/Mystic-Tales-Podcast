using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.sqlserver;

public partial class AccountPodcastListenHistory
{
    public int AccountId { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
