using SubscriptionService.BusinessLogic.DTOs.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MemberSubscription.ListItems
{
    public class MemberSubscriptionCycleTypePriceListItemResponseDTO
    {
        public int MemberSubscriptionId { get; set; }
        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; }
        public int Version { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
