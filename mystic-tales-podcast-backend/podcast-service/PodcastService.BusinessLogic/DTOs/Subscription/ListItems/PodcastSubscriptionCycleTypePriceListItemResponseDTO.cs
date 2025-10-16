using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Subscription.ListItems
{
    public class PodcastSubscriptionCycleTypePriceListItemResponseDTO
    {
        public int PodcastSubscriptionId { get; set; }

        public int SubscriptionCycleTypeId { get; set; }

        public int Version { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; } = null!;
    }
}