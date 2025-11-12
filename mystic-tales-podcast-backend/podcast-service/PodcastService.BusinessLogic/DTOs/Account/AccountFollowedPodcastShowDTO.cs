using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.Account;

public partial class AccountFollowedPodcastShowDTO
{
    public int AccountId { get; set; }

    public Guid PodcastShowId { get; set; }

    public DateTime CreatedAt { get; set; }

    public AccountDTO Account { get; set; } = null!;
}
