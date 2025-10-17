using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class ChannelCreateRequestDTO
    {
        public required string ChannelCreateInfo { get; set; }
        public IFormFile? BackgroundImageFile { get; set; }
        public IFormFile? MainImageFile { get; set; }
    }
    public class ChannelCreateInfoDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string PodcasterId { get; set; } = null!;
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public List<int>? HashtagIds { get; set; }

    }
}