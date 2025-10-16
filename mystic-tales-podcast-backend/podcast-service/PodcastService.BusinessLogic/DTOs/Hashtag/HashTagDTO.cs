using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Hashtag
{
    public class HashtagDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}