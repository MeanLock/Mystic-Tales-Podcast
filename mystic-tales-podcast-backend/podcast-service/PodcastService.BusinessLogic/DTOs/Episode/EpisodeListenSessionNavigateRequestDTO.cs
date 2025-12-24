using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.PodcastSubscription;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenSessionNavigateRequestDTO
    {
        public required EpisodeListenSessionNavigateCurrentListenSessionDTO CurrentListenSession { get; set; } = null!;
        public required List<PodcastSubscriptionBenefitDTO>? CurrentPodcastSubscriptionRegistrationBenefitList { get; set; }
    }

    public class EpisodeListenSessionNavigateCurrentListenSessionDTO
    {
        public required Guid ListenSessionId { get; set; }
        public required Guid ListenSessionProcedureId { get; set; }
    }
}