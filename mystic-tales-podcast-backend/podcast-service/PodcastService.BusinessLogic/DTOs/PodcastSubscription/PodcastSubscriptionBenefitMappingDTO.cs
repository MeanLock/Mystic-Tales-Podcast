using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionBenefitMappingDTO
    {
        public int PodcastSubscriptionId { get; set; }

        public int PodcastSubscriptionBenefitId { get; set; }

        public int Version { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public PodcastSubscriptionDTO PodcastSubscription { get; set; } = null!;

        public PodcastSubscriptionBenefitDTO PodcastSubscriptionBenefit { get; set; } = null!;
    }
}
