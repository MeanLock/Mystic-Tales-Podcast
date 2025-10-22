using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeUpdateRequestDTO
    {
        public required string EpisodeUpdateInfo { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
    }
    public class EpisodeUpdateInfoDTO
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool ExplicitContent { get; set; }

        public int PodcastEpisodeSubscriptionTypeId { get; set; }

        public int SeasonNumber { get; set; }
        public int EpisodeOrder { get; set; }
        public List<int>? HashtagIds { get; set; }

    }
}