using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenSessionResponseDTO
    {
        public required PodcastEpisodeSnippetResponseDTO PodcastEpisode { get; set; } = null!;
        public required AccountSnippetResponseDTO Podcaster { get; set; } = null!;
        public required PodcastEpisodeListenSessionSnippetResponseDTO PodcastEpisodeListenSession { get; set; } = null!;
        public required string PlaylistFileKey { get; set; } = null!;
        public required string Token { get; set; } = null!;
        public required string? AudioFileUrl { get; set; }
    }
}