using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction
{
    public class AccountBalanceTransactionCreateRequestDTO
    {
        public AccountBalanceTransactionCreateInfoDTO AccountBalanceTransactionCreateInfo { get; set; }
    }
    public class AccountBalanceTransactionCreateInfoDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
