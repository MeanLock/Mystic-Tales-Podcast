using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Account
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
        public required int PodcasterId { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? podcastSubCategoryId { get; set; }
        
    }
}