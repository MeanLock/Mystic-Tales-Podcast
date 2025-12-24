using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems
{
    public class PodcastSubscriptionBenefitMappingListItemResponseDTO
    {
        public int PodcastSubscriptionId { get; set; }
        public PodcastSubscriptionBenefitDTO PodcastSubscriptionBenefit { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
