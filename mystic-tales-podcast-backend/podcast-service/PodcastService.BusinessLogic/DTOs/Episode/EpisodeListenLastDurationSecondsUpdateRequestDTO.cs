using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.PodcastSubscription;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenLastDurationSecondsUpdateRequestDTO
    {
        public required List<PodcastSubscriptionBenefitDTO> CurrentPodcastSubscriptionRegistrationBenefitList { get; set; }
    }
}