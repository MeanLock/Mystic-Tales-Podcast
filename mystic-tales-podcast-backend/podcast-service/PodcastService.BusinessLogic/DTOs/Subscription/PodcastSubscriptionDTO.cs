using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Subscription
{
    public class PodcastSubscriptionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public Guid? PodcastChannelId { get; set; }

        public Guid? PodcastShowId { get; set; }

        public bool IsActive { get; set; }

        public int CurrentVersion { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}