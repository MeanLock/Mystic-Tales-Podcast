using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPaymentRollback
{
    public class ConfirmPaymentRollbackParameterDTO
    {
        public Guid AccountBalanceTransactionId { get; set; }
    }
}
