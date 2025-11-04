using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateWithdrawalRequest
{
    public class CreateWithdrawalRequestParameterDTO
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
