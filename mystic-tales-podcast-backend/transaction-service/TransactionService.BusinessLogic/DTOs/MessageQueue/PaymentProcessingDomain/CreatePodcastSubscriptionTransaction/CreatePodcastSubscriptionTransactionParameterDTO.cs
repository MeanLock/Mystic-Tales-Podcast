using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransaction
{
    public class CreatePodcastSubscriptionTransactionParameterDTO
    {
        public Guid PodcastSubscriptionRegistrationId { get; set; }
        public decimal? Profit { get; set; }
        public int? AccountId { get; set; }
        public int? PodcasterId { get; set; }
        public decimal Amount { get; set; }
        public int TransactionTypeId { get; set; }
    }
}
