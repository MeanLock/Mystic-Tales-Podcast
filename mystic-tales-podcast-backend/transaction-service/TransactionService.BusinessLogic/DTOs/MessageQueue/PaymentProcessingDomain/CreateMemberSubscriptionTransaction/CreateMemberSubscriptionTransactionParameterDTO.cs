using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransaction
{
    public class CreateMemberSubscriptionTransactionParameterDTO
    {
        public Guid MemberSubscriptionRegistrationId { get; set; }
        public int? AccountId { get; set; }
        public decimal Amount { get; set; }
        public int TransactionTypeId { get; set; }
    }
}
