using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.Account;

public class AccountFavoritedPodcastChannelDTO
{
    public int AccountId { get; set; }

    public Guid PodcastChannelId { get; set; }

    public DateTime CreatedAt { get; set; }

    public AccountDTO Account { get; set; } = null!;
}
