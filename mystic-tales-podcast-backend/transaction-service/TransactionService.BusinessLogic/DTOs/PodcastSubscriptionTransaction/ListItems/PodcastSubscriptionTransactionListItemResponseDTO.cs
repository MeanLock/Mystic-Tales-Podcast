using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.Transaction;

namespace TransactionService.BusinessLogic.DTOs.PodcastSubscriptionTransaction.ListItems
{
    public class PodcastSubscriptionTransactionListItemResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PodcastSubscriptionRegistrationId { get; set; }
        public decimal Amount { get; set; }
        public TransactionTypeResponseDTO TransactionType { get; set; }
        public TransactionStatusResponseDTO TransactionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
}
