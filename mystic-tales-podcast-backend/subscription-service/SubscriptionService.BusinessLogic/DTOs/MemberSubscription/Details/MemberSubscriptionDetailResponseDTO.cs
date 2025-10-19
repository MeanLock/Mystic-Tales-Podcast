using SubscriptionService.BusinessLogic.DTOs.MemberSubscription.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MemberSubscription.Details
{
    public class MemberSubscriptionDetailResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int CurrentVersion { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MemberSubscriptionCycleTypePriceListItemResponseDTO> MemberSubscriptionCycleTypePriceList { get; set; }
        public List<MemberSubscriptionBenefitMappingListItemResponseDTO> MemberSubscriptionBenefitMappingList { get; set; }
        public List<MemberSubscriptionRegistrationListItemResponseDTO> MemberSubscriptionRegistrationList { get; set; }
    }
}
