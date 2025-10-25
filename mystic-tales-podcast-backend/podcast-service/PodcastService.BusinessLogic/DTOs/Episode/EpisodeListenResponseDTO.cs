using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Cachegory;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenResponseDTO
    {
        public required PodcastEpisodeSnippetResponseDTO PodcastEpisode { get; set; } = null!;
        public required AccountSnippetResponseDTO Podcaster { get; set; } = null!;
        public required string PlaylistFileKey { get; set; } = null!;
    }
}