using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Subscription.ListItems
{
    public class PodcastSubscriptionBenefitMappingListItemResponseDTO
    {
        public int PodcastSubscriptionId { get; set; }

        public PodcastSubscriptionBenefitDTO PodcastSubscriptionBenefit { get; set; } = null!;

        public int Version { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}