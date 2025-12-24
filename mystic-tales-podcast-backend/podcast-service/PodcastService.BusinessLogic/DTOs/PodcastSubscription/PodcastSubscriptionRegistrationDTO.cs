using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.PodcastSubscription
{
    public class PodcastSubscriptionRegistrationDTO
    {
        public Guid Id { get; set; }

        public int? AccountId { get; set; }

        public int PodcastSubscriptionId { get; set; }

        public int SubscriptionCycleTypeId { get; set; }

        public int CurrentVersion { get; set; }

        public bool? IsAcceptNewestVersionSwitch { get; set; }

        public DateTime LastPaidAt { get; set; }

        public bool IsIncomeTaken { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public PodcastSubscriptionDTO PodcastSubscription { get; set; } = null!;

        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; } = null!;
    }
}
