using SubscriptionService.BusinessLogic.DTOs.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MemberSubscription.ListItems
{
    public class MemberSubscriptionRegistrationListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public int MemberSubscriptionId { get; set; }
        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; }
        public int CurrentVersion { get; set; }
        public bool? IsAcceptNewestVersionSwitch { get; set; }
        public bool IsIncomeTaken { get; set; }
        public DateTime LastPaidAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
