using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionCycleTypePriceDTO
    {
        public int PodcastSubscriptionId { get; set; }

        public int SubscriptionCycleTypeId { get; set; }

        public int Version { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public PodcastSubscriptionDTO PodcastSubscription { get; set; } = null!;

        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; } = null!;
    }
}
