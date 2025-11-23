using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems
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
