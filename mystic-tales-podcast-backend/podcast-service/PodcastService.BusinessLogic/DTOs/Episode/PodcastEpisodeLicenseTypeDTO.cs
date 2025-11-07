using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class PodcastEpisodeLicenseTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}