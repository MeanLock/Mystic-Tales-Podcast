using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransactionRollback
{
    public class CreateMemberSubscriptionTransactionRollbackParameterDTO
    {
        public Guid MemberSubscriptionTransactionId { get; set; }
    }
}
