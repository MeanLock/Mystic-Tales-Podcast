using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class ChannelDeleteRequestDTO
    {
        public required ChannelDeletionOptionsDTO ChannelDeletionOptions { get; set; }
    }

    public class ChannelDeletionOptionsDTO
    {
        public List<Guid> KeptShowIds { get; set; } = new List<Guid>();
    }
}