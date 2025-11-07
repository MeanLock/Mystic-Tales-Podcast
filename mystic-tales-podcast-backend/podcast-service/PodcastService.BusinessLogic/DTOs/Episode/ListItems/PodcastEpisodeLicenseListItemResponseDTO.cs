using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show;

namespace PodcastService.BusinessLogic.DTOs.Episode.ListItems
{
    public class PodcastEpisodeLicenseListItemResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PodcastEpisodeId { get; set; }
        public string LicenseDocumentFileKey { get; set; } = null!;
        public required PodcastEpisodeLicenseTypeDTO PodcastEpisodeLicenseType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

    }
}