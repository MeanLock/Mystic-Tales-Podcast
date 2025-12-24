using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Transaction
{
    public class PodcastSubscriptionTransactionListItemDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? Profit { get; set; }
        public TransactionTypeDTO TransactionType { get; set; }
        public TransactionStatusDTO TransactionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid PodcastSubscriptionRegistrationId { get; set; }
    }
}
