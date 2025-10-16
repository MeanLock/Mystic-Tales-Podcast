using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Cachegory;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class PodcastShowSubscriptionTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}