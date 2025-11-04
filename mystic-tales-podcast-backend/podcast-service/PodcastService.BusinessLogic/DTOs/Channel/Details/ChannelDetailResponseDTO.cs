using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Subscription.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Channel.Details
{
    public class ChannelDetailResponseDTO : ChannelListItemResponseDTO
    {
        public List<PodcastSubscriptionListItemResponseDTO> PodcastSubscriptionList { get; set; } = new();
        public List<ShowListItemResponseDTO> ShowList { get; set; } = new();



    }
}
