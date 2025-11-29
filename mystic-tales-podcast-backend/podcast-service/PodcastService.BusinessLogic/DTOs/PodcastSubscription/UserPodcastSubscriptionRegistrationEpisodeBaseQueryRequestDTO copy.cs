using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO
    {
        public required List<UserPodcastSubscriptionRegistrationEpisodeBaseBenefitListItemResponseDTO> EpisodeBaseBenefitList { get; set; }
    }

    public class UserPodcastSubscriptionRegistrationEpisodeBaseBenefitListItemResponseDTO
    {
        public required Guid EpisodeId { get; set; }
        public required List<int> PodcastSubscriptionBenefitIds { get; set; }
    }
}
