using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink
{
    public class AccountBalanceCreatePaymentLinkParameterDTO
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
