using Microsoft.AspNetCore.Http;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast
{
    public class PodcastShowSubscriptionTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}