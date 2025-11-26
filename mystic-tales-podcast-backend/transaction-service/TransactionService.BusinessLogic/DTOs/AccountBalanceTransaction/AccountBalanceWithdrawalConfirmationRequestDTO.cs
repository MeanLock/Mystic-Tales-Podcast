using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction
{
    public class AccountBalanceWithdrawalConfirmationRequestDTO
    {
        public IFormFile? TransferReceiptImageFile { get; set; }
        public string AccountBalanceWithdrawalRequestInfo { get; set; }
    }
    public class AccountBalanceWithdrawalRequestInfoDTO
    {
        public string? RejectedReason { get; set; }
    }
}
