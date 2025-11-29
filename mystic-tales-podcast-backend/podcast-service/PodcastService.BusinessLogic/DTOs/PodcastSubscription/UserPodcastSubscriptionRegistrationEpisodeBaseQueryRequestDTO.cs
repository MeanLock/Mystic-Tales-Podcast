using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class UserPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO
    {
        public required List<EpisodeBaseSourceInfoDTO> EpisodeBaseSourceInfoList { get; set; }
    }

    public class EpisodeBaseSourceInfoDTO
    {
        public required Guid EpisodeId { get; set; }
        public required Guid ShowId { get; set; }
        public required Guid? ChannelId { get; set; }
    }
}
