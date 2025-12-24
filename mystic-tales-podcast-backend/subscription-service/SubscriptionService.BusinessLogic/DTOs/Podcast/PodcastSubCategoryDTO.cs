using Microsoft.AspNetCore.Http;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast
{
    public class PodcastSubCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int PodcastCategoryId { get; set; }
    }
}