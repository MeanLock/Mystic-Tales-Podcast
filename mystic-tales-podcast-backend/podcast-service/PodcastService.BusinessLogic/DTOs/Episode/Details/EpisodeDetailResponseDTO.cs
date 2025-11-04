using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Subscription.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Episode.Details
{
    public class EpisodeDetailResponseDTO : EpisodeListItemResponseDTO
    {

        public required AccountSnippetResponseDTO Podcaster { get; set; } = null!;


    }
}
