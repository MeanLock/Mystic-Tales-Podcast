using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MemberSubscription
{
    public class MemberSubscriptionCycleTypePriceCreateRequestDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
