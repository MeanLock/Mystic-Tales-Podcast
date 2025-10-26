using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
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

        public ICollection<PodcastSubscriptionBenefitMappingDTO> PodcastSubscriptionBenefitMappings { get; set; } = new List<PodcastSubscriptionBenefitMappingDTO>();

        public ICollection<PodcastSubscriptionCycleTypePriceDTO> PodcastSubscriptionCycleTypePrices { get; set; } = new List<PodcastSubscriptionCycleTypePriceDTO>();

        public ICollection<PodcastSubscriptionRegistrationDTO> PodcastSubscriptionRegistrations { get; set; } = new List<PodcastSubscriptionRegistrationDTO>();
    }
}
