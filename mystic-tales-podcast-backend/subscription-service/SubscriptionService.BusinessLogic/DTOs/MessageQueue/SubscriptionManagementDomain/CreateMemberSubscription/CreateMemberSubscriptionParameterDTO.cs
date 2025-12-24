using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateMemberSubscription
{
    public class CreateMemberSubscriptionParameterDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<MemberSubscriptionCycleTypePriceParameterDTO> MemberSubscriptionCycleTypePriceList { get; set; }
        public List<int> MemberSubscriptionBenefitMappingList { get; set; }
    }
    public class MemberSubscriptionCycleTypePriceParameterDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
