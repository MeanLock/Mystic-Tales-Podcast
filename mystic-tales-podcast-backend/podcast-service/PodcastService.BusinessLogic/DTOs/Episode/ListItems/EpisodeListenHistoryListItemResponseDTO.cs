using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show;

namespace PodcastService.BusinessLogic.DTOs.Episode.ListItems
{
    public class EpisodeListenHistoryListItemResponseDTO
    {
        public required PodcastEpisodeSnippetResponseDTO PodcastEpisode { get; set; } = null!;
        public required AccountSnippetResponseDTO Podcaster { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}