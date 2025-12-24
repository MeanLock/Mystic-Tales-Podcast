using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionUpdateRequestDTO
    {
        public PodcastSubscriptionUpdateInfoDTO PodcastSubscriptionUpdateInfo { get; set; }
    }
    public class PodcastSubscriptionUpdateInfoDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PodcastSubscriptionCycleTypePriceUpdateRequestDTO> PodcastSubscriptionCycleTypePriceUpdateInfoList { get; set; }
        public List<int> PodcastSubscriptionBenefitMappingUpdateInfoList { get; set; }
    }
}
