using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionCreateRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PodcastSubscriptionCycleTypePriceCreateRequestDTO> PodcastSubscriptionCycleTypePriceCreateInfoList { get; set; }
        public List<int> PodcastSubscriptionBenefitMappingCreateInfoList { get; set; }
    }
    public class PodcastSubscriptionCycleTypePriceCreateRequestDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
