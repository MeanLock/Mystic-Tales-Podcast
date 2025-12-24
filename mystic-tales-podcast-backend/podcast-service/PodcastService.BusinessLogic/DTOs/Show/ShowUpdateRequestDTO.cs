using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class ShowUpdateRequestDTO
    {
        public required string ShowUpdateInfo { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
    }
    public class ShowUpdateInfoDTO
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Language { get; set; } = null!;

        public string Copyright { get; set; } = null!;

        public string? UploadFrequency { get; set; }

        public int? PodcastCategoryId { get; set; }

        public int? PodcastSubCategoryId { get; set; }

        public int PodcastShowSubscriptionTypeId { get; set; }

        // public Guid? PodcastChannelId { get; set; }
        
        public List<int>? HashtagIds { get; set; }
    }
}