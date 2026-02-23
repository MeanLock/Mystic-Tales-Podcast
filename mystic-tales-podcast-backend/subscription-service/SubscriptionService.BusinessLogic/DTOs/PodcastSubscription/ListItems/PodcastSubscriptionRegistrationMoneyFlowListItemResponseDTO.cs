using SubscriptionService.BusinessLogic.DTOs.Snippet;
using SubscriptionService.BusinessLogic.DTOs.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubscriptionService.BusinessLogic.DTOs.Transaction;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems
{
    public class PodcastSubscriptionRegistrationMoneyFlowListItemResponseDTO
    {
        public Guid Id { get; set; }
        public AccountSnippetResponseDTO Account { get; set; }
        public int PodcastSubscriptionId { get; set; }
        public SubscriptionCycleTypeDTO SubscriptionCycleType { get; set; }
        public int CurrentVersion { get; set; }
        public bool? IsAcceptNewestVersionSwitch { get; set; }
        public bool IsIncomeTaken { get; set; }
        public DateTime LastPaidAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal HoldingAmount { get; set; }
        public decimal ProfitAmount { get; set; }
        public List<PodcastSubscriptionTransactionListItemDTO> PodcastSubscriptionTransactionList { get; set; }
    }
}
