using SubscriptionService.BusinessLogic.DTOs.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.Details
{
    public class PodcastSubscriptionRegistrationDetailResponseDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public int PodcastSubscriptionId { get; set; }
        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; }
        public decimal Price { get; set; }
        public int CurrentVersion { get; set; }
        public bool? IsAcceptNewestVersionSwitch { get; set; }
        public bool IsIncomeTaken { get; set; }
        public DateTime LastPaidAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastSubscriptionBenefitDTO> PodcastSubscriptionBenefitList { get; set; }
    }
}
