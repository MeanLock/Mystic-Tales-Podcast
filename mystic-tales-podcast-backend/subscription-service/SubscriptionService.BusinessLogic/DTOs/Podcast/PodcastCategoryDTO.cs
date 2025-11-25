using Microsoft.AspNetCore.Http;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast
{
    public class PodcastCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? MainImageFileKey { get; set; }
    }
}