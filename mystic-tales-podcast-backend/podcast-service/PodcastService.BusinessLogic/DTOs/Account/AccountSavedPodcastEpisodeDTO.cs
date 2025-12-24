using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.Account;

public partial class AccountSavedPodcastEpisodeDTO
{
    public int AccountId { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public AccountDTO Account { get; set; } = null!;
}
