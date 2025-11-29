using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.PodcastSubscription;
using PodcastService.BusinessLogic.Enums.ListenSessionProcedure;
using System.ComponentModel.DataAnnotations;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenRequestDTO
    {
        [Required]
        [EnumDataType(typeof(CustomerListenSessionProcedureSourceDetailTypeEnum))]
        public required CustomerListenSessionProcedureSourceDetailTypeEnum SourceType { get; set; }
        public required List<PodcastSubscriptionBenefitDTO>? CurrentPodcastSubscriptionRegistrationBenefitList { get; set; }
    }
}