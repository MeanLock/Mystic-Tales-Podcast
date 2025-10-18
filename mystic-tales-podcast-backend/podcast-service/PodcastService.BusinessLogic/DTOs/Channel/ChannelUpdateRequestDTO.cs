using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class ChannelUpdateRequestDTO
    {
        public required string ChannelUpdateInfo { get; set; }
        public IFormFile? BackgroundImageFile { get; set; }
        public IFormFile? MainImageFile { get; set; }
    }
    public class ChannelUpdateInfoDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public List<int>? HashtagIds { get; set; }

    }
}