using SubscriptionService.BusinessLogic.DTOs.MemberSubscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription
{
    public class MemberSubscriptionCreateRequestDTO
    {
        public MemberSubscriptionCreateInfoDTO MemberSubscriptionCreateInfo { get; set; }
    }
    public class MemberSubscriptionCreateInfoDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<MemberSubscriptionCycleTypePriceCreateRequestDTO> MemberSubscriptionCycleTypePriceCreateInfoList { get; set; }
        public List<int> MemberSubscriptionBenefitMappingCreateInfoList { get; set; }
    }
}
