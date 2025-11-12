using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Hashtag
{
    public class HashtagCreateRequestDTO
    {
        public required string HashtagName { get; set; }
    }
}