using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateAccountBalanceTransactionRollback
{
    public class CreateAccountBalanceTransactionRollbackParameterDTO
    {
        public Guid AccountBalanceTransactionId { get; set; }
    }
}
