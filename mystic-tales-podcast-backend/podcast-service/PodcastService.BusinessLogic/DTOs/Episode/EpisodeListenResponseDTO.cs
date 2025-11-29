using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeListenResponseDTO
    {
        public required EpisodeListenSessionResponseDTO ListenSession { get; set; } = null!;
        public required CustomerListenSessionProcedure ListenSessionProcedure { get; set; } = null!;
    }
}