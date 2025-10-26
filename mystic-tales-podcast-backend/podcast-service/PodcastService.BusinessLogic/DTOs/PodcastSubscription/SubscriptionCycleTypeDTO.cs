using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class SubscriptionCycleTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<PodcastSubscriptionCycleTypePriceDTO> PodcastSubscriptionCycleTypePrices { get; set; } = new List<PodcastSubscriptionCycleTypePriceDTO>();

        public ICollection<PodcastSubscriptionRegistrationDTO> PodcastSubscriptionRegistrations { get; set; } = new List<PodcastSubscriptionRegistrationDTO>();
    }
}
