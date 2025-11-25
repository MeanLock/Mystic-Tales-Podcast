using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Transaction
{
    public class PodcastSubscriptionTransactionDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? Profit { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid PodcastSubscriptionRegistrationId { get; set; }
    }
}
