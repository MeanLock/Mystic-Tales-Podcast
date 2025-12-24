using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmAccountBalanceWithdrawal
{
    public class ConfirmAccountBalanceWithdrawalParameterDTO
    {
        public Guid AccountBalanceWithdrawalRequestId { get; set; }
        public string? ImageFileKey { get; set; }
        public string? RejectedReason { get; set; }
        public bool IsReject { get; set; }
    }
}
