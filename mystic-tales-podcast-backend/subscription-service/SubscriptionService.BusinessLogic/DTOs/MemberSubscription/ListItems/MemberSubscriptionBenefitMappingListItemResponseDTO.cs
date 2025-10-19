using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MemberSubscription.ListItems
{
    public class MemberSubscriptionBenefitMappingListItemResponseDTO
    {
        public int PodcastSubscriptionId { get; set; }
        public MemberSubscriptionBenefitDTO MemberSubscriptionBenefit { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
