using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteMemberSubscriptionTransaction
{
    public class CompleteMemberSubscriptionTransactionParameterDTO
    {
        public Guid MemberSubscriptionTransactionId { get; set; }
    }
}
