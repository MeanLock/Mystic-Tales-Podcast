using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompletePodcastSubscriptionTransaction
{
    public class CompletePodcastSubscriptionTransactionParameterDTO
    {
        public Guid PodcastSubscriptionTransactionId { get; set; }
    }
}
