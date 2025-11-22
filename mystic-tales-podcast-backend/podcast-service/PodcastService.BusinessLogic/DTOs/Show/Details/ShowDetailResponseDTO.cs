using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Subscription.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Show.Details
{
    public class ShowDetailResponseDTO : ShowListItemResponseDTO
    {
        public List<PodcastSubscriptionListItemResponseDTO> PodcastSubscriptionList { get; set; } = new();
        public List<EpisodeListItemResponseDTO> EpisodeList { get; set; } = new();
        public required List<PodcastShowReviewListItemResponseDTO> ReviewList { get; set; } = new List<PodcastShowReviewListItemResponseDTO>();


    }
}
