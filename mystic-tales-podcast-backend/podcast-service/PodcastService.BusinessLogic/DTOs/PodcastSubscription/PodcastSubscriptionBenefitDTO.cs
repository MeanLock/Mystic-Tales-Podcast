using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionBenefitDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<PodcastSubscriptionBenefitMappingDTO> PodcastSubscriptionBenefitMappings { get; set; } = new List<PodcastSubscriptionBenefitMappingDTO>();
    }
}
