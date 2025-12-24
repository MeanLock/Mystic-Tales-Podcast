using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class ShowChannelAssignRequestDTO
    {
        public Guid? PodcastChannelId { get; set; }
    }
}